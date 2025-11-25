using System.Security.Claims;
using Microsoft.AspNet.SignalR.Client.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.HubSignalR;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Controllers;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class IGDTriageController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<IGDTriageController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHubContext<IGDTriageHub> _hubContext;

        public IGDTriageController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<IGDTriageController> logger,
            IWebHostEnvironment webHostEnvironment,
            IHubContext<IGDTriageHub> hubContext)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = (from a in _applicationDbContext.IGDTriages
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.TriageId,
                             a.KunjunganId,
                             a.KeluhanUtama,
                             a.DiteruskanKepada,
                             a.WaktuMasuk,
                             a.Keterangan,
                         }).OrderByDescending(a => a.CreateDateTime);

            // Hitung total data sebelum paginasi
            var totalRows = query.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            // Ambil data sesuai paging
            var listdata = query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToList();

            if (!listdata.Any())
            {
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });
            }

            // Return hasil dengan paging info
            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data = listdata,
                pagination = new
                {
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalRows = totalRows,
                    TotalPages = totalPages
                }
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            try
            {
                // ✅ Query utama join langsung (tanpa N+1)
                var baseQuery = from t in _applicationDbContext.IGDTriages
                                join d in _applicationDbContext.IGDTriageDetails
                                    on t.TriageId equals d.TriageId into detailGroup
                                from d in detailGroup.DefaultIfEmpty()
                                where (t.IsDelete == false || t.IsDelete == null)
                                      && t.TriageId == id
                                select new
                                {
                                    TriageId = (Guid?)t.TriageId,
                                    KunjunganId = (Guid?)t.KunjunganId,
                                    t.KeluhanUtama,
                                    t.DiteruskanKepada,
                                    t.Keterangan,
                                    DetailIndikatorId = (Guid?)d.IndikatorPengkajianId,
                                    DetailKeterangan = d != null ? d.Keterangan : null,
                                    DetailCreateTime = (DateTimeOffset?)d.CreateDateTime,
                                    t.CreateBy,
                                    CreateDateTime = (DateTimeOffset?)t.CreateDateTime
                                };

                // 🔹 Eksekusi query ke memory dulu (EF tidak bisa GroupBy langsung dengan DefaultIfEmpty)
                var data = await baseQuery.ToListAsync();

                if (data == null || data.Count == 0)
                {
                    return NotFound(new { message = "Data tidak ditemukan untuk ID tersebut." });
                }

                // 🔹 Grouping detail (1 Triage -> banyak detail)
                var result = data
                    .GroupBy(x => new
                    {
                        x.TriageId,
                        x.KunjunganId,
                        x.KeluhanUtama,
                        x.DiteruskanKepada,
                        x.Keterangan,
                        x.CreateBy,
                        x.CreateDateTime
                    })
                    .Select(g => new
                    {
                        g.Key.TriageId,
                        g.Key.KunjunganId,
                        g.Key.KeluhanUtama,
                        g.Key.DiteruskanKepada,
                        g.Key.Keterangan,
                        g.Key.CreateBy,
                        CreateDateTime = g.Key.CreateDateTime ?? DateTimeOffset.MinValue,
                        Details = g
                            .Where(x => x.DetailIndikatorId != null)
                            .Select(x => new
                            {
                                IndikatorPengkajianId = x.DetailIndikatorId,
                                Keterangan = x.DetailKeterangan
                            }).ToList()
                    })
                    .FirstOrDefault();

                return Ok(new
                {
                    status = "success",
                    message = "Data retrieved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] IGDTriageViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // **Cek koneksi ke database**
                if (!_applicationDbContext.Database.CanConnect())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // **Ambil User ID dari JWT Claims**
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
                if (getUserActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }
                var userActiveId = getUserActive.UserActiveId;

                ////// **Cek Duplikasi**
                //bool isDuplicate = await _applicationDbContext.Diskons
                //                    .AnyAsync(c => c.NamaDiskon == vm.NamaDiskon);

                //if (isDuplicate)
                //{
                //    return Conflict(new { message = "Nama benefit ini telah tersedia" });
                //}

                // **Buat Data Baru**
                var data = new IGDTriage
                {
                    TriageId = Guid.NewGuid(),
                    KunjunganId = vm.KunjunganId,
                    KeluhanUtama = vm.KeluhanUtama,
                    DiteruskanKepada = vm.DiteruskanKepada,
                    WaktuMasuk = DateTime.Now,
                    Keterangan = vm.Keterangan,

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                // **Simpan ke Database**
                _applicationDbContext.IGDTriages.Add(data);
                // 💾 Simpan data detail triage
                // =============================
                if (vm.Details != null && vm.Details.Count > 0)
                {
                    foreach (var detail in vm.Details)
                    {
                        var detailEntity = new IGDTriageDetail
                        {
                            DetailTriageId = Guid.NewGuid(),
                            TriageId = data.TriageId,
                            IndikatorPengkajianId = detail.IndikatorPengkajianId,
                            Keterangan = detail.Keterangan,
                            CreateDateTime = DateTimeOffset.UtcNow,
                            CreateBy = userActiveId,
                            IsDelete = false
                        };

                        _applicationDbContext.IGDTriageDetails.Add(detailEntity);
                    }
                }
                int result = await _applicationDbContext.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("Triage Created", new
                {
                    Action = "create",
                    TriageId = data.TriageId
                });
                if (result > 0)
                {
                    return Created("", new { message = "Tambah Data Berhasil || 201 Created" });
                }
                else
                {
                    return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
                }
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal menyimpan data: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] IGDTriageViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // **Cek koneksi ke database**
                if (!_applicationDbContext.Database.CanConnect())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // **Ambil User ID dari JWT Claims**
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
                if (getUserActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }
                var userActiveId = getUserActive.UserActiveId;

                // **Cari Data Lama**
                var data = await _applicationDbContext.IGDTriages.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data permintaan darah tidak ditemukan." });
                }

                // **Update Data**
                data.KunjunganId = vm.KunjunganId;
                data.WaktuMasuk = DateTime.Now;
                data.DiteruskanKepada = vm.DiteruskanKepada;
                data.KeluhanUtama = vm.KeluhanUtama;
                data.Keterangan = vm.Keterangan;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.IGDTriages.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();


                await _hubContext.Clients.All.SendAsync("Triage Changed", new
                {
                    Action = "change",
                    TriageId = data.TriageId
                });
                if (result > 0)
                {
                    return Ok(new { message = "Update Data Berhasil || 200 OK" });
                }
                else
                {
                    return StatusCode(500, new { message = "Data tidak berhasil diperbarui di database." });
                }
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal memperbarui data: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                // **Cek koneksi ke database**
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // **Ambil User ID dari JWT Claims**
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (getUserActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }
                var userActiveId = getUserActive.UserActiveId;

                // **Cari Data**
                var data = await _applicationDbContext.IGDTriages.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.IGDTriages.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Data berhasil dihapus (soft delete) || 200 OK" });
                }
                else
                {
                    return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
                }
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal menghapus data: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpGet("paged")]
        public async Task<IActionResult> PagedAsync(
            int page = 1,
            int perPage = 10,
            Guid? kunjunganId = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                                    DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                                    DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {

            // Query join langsung, tanpa subquery
            var baseQuery = from t in _applicationDbContext.IGDTriages
                            join d in _applicationDbContext.IGDTriageDetails
                                on t.TriageId equals d.TriageId into detailGroup
                            from d in detailGroup.DefaultIfEmpty()
                            where (t.IsDelete == false || t.IsDelete == null)
                            select new
                            {
                                TriageId = (Guid?)t.TriageId, // 🩹 cast ke nullable
                                KunjunganId = (Guid?)t.KunjunganId, // 🩹 cast ke nullable
                                t.KeluhanUtama,
                                t.DiteruskanKepada,
                                t.Keterangan,
                                DetailIndikatorId = (Guid?)d.IndikatorPengkajianId, // 🩹 cast ke nullable
                                DetailKeterangan = d != null ? d.Keterangan : null,
                                DetailCreateTime = (DateTimeOffset?)d.CreateDateTime, // 🩹 nullable timestamp
                                t.CreateBy,
                                CreateDateTime = (DateTimeOffset?)t.CreateDateTime // 🩹 nullable timestamp
                            };


            var grouped = await baseQuery
                .OrderByDescending(x => x.CreateDateTime)
                .ToListAsync();

            // Grouping di memory (karena EF belum bisa langsung GroupBy-to-List dengan navigation)
            var result = grouped
                .GroupBy(x => new
                {
                    x.TriageId,
                    x.KunjunganId,
                    x.KeluhanUtama,
                    x.DiteruskanKepada,
                    x.Keterangan,
                    x.CreateBy,
                    x.CreateDateTime,
                })
                .Select(g => new
                {
                    g.Key.TriageId,
                    g.Key.KunjunganId,
                    g.Key.KeluhanUtama,
                    g.Key.DiteruskanKepada,
                    g.Key.Keterangan,
                    g.Key.CreateBy,
                    CreateDateTime = g.Key.CreateDateTime ?? DateTimeOffset.MinValue,
                    Details = g
                        .Where(x => x.DetailIndikatorId != null)
                        .Select(x => new
                        {
                            IndikatorPengkajianId = x.DetailIndikatorId,
                            Keterangan = x.DetailKeterangan
                        }).ToList()
                }).AsQueryable();

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            //if (!string.IsNullOrWhiteSpace(search))
            //{
            //    search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
            //    query = query.Where(u =>
            //        EF.Functions.ILike(u.NamaDiskon, search)
            //    );
            //}

            // filter berdasarkan kunjungan id
            if (kunjunganId.HasValue)
            {
                result = result.Where(u => u.KunjunganId == kunjunganId.Value);
            }

            //// **Filter berdasarkan tanggal**
            if (startDate.HasValue && endDate.HasValue)
            {
                DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                result = result.Where(u =>
                    u.CreateDateTime >= startUtc &&
                    u.CreateDateTime <= endUtc);
            }

            // Filter berdasarkan periode (Hari Ini, Minggu Ini, dll) hanya jika periode memiliki nilai
            if (periode.HasValue)
            {
                DateTime today = DateTime.UtcNow.Date;

                switch (periode)
                {
                    case PeriodeFilter.Today:
                        result = result.Where(u => u.CreateDateTime.Date == today);
                        break;
                    case PeriodeFilter.ThisWeek:
                        result = result.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-(int)today.DayOfWeek) &&
                            u.CreateDateTime.Date <= today
                        );
                        break;
                    case PeriodeFilter.LastWeek:
                        result = result.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                            u.CreateDateTime.Date < today.AddDays(-(int)today.DayOfWeek)
                        );
                        break;
                    case PeriodeFilter.ThisMonth:
                        result = result.Where(u =>
                            u.CreateDateTime.Month == today.Month &&
                            u.CreateDateTime.Year == today.Year
                        );
                        break;
                    case PeriodeFilter.LastMonth:
                        result = result.Where(u =>
                            u.CreateDateTime.Month == today.Month - 1 &&
                            u.CreateDateTime.Year == today.Year
                        );
                        break;
                    case PeriodeFilter.ThisYear:
                        result = result.Where(u => u.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.LastYear:
                        result = result.Where(u => u.CreateDateTime.Year == today.Year - 1);
                        break;
                    case PeriodeFilter.Last3Months:
                        result = result.Where(u => u.CreateDateTime >= today.AddMonths(-3));
                        break;
                    case PeriodeFilter.Last6Months:
                        result = result.Where(u => u.CreateDateTime >= today.AddMonths(-6));
                        break;
                }
            }

            // Sorting Data dengan cara yang lebih aman
            result = sortDirection?.ToLower() == "desc"
                ? orderBy switch
                {
                    "CreateDateTime" => result.OrderByDescending(u => u.CreateDateTime),
                    _ => result.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => result.OrderBy(u => u.CreateDateTime),
                    _ => result.OrderBy(u => u.CreateDateTime)
                };

            // Pagination
            var totalRows = result.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);
            var rows = result.Skip((page - 1) * perPage).Take(perPage).ToList();

            if (rows.Count == 0 && page > totalPages)
            {
                return NotFound(new { message = "Page not found." });
            }

            return Ok(new
            {
                status = "success",
                message = "Data retrieved successfully",
                data = new
                {
                    Rows = rows,
                    TotalRows = totalRows,
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalPages = totalPages
                }
            });
        }
    }
}
