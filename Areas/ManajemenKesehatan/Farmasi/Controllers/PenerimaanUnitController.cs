using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.HubSignalR;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class PenerimaanUnitController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<PenerimaanUnitController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHubContext<PermintaanUnitHub> _hubContext;

        public PenerimaanUnitController
            (
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<PenerimaanUnitController> logger,
            IWebHostEnvironment webHostEnvironment,
            IHubContext<PermintaanUnitHub> hubContext
            )
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _hubContext = hubContext;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        private DateTime? TryParseTanggalToUtc(string tanggal)
        {
            if (DateTime.TryParseExact(
                    tanggal,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsedDate))
            {
                var now = DateTime.Now; // atau DateTime.UtcNow jika kamu mau jam UTC
                var finalDateTime = new DateTime(
                    parsedDate.Year,
                    parsedDate.Month,
                    parsedDate.Day,
                    now.Hour,
                    now.Minute,
                    now.Second,
                    DateTimeKind.Local); // atau Utc jika perlu

                return finalDateTime.ToUniversalTime(); // simpan dalam UTC
            }

            return null;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = from a in _applicationDbContext.PenerimaanUnits
                        join u in _applicationDbContext.UserActives
                            on a.CreateBy equals u.UserActiveId into ua
                        from u in ua.DefaultIfEmpty()

                            // GroupJoin ke DetailPenerimaanUnits
                        join d in _applicationDbContext.DetailPenerimaanUnits
                            on a.PenerimaanUnitId equals d.PenerimaanUnitId into detailJoin
                        from d in detailJoin.DefaultIfEmpty()

                            // Left join ke Obats
                        join ob in _applicationDbContext.Obats
                            on d.ObatId equals ob.ObatId into obatJoin
                        from ob in obatJoin.DefaultIfEmpty()

                            // Left join ke BentukObats
                        join b in _applicationDbContext.BentukObats
                            on ob.BentukObatId equals b.BentukObatId into bentukJoin
                        from b in bentukJoin.DefaultIfEmpty()

                        where a.IsDelete == false || a.IsDelete == null
                        select new
                        {
                            a.CreateDateTime,
                            a.CreateBy,
                            CreateByName = u.FullName,
                            a.PenerimaanUnitId,
                            a.UnitId,
                            //a.JenisPermintaan,
                            a.TglPenerimaan,
                            a.StatusPenerimaan,
                            a.Keterangan,

                            Detail = d == null ? null : new
                            {
                                d.DetailPenerimaanUnitId,
                                d.ObatId,
                                NamaObat = ob != null ? ob.ObatName : null,
                                Bentuk = b != null ? b.NamaBentukObat : null,
                                StokObat = ob != null ? ob.Stock : 0,
                                StockMinimal = ob != null ? ob.Minimal : 0,
                                StockMaksimal = ob != null ? ob.Maximal : 0,
                                Dosis = ob != null ? ob.TakaranDosis : 0,
                                HTE = ob != null ? ob.HTEPrice : 0,
                                d.QtyPermintaan,
                                d.SatuanItem,
                                d.KategoriItem,
                                d.Keterangan
                            }
                        };

            // Hitung total data
            var totalRows = await query
                .GroupBy(x => new
                {
                    x.PenerimaanUnitId,
                    x.CreateDateTime,
                    x.CreateBy,
                    x.CreateByName,
                    x.UnitId,
                    //x.JenisPermintaan,
                    x.TglPenerimaan,
                    x.StatusPenerimaan,
                    x.Keterangan
                })
                .CountAsync();

            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            // Ambil data sesuai paging
            var listdata = await query
                .GroupBy(x => new
                {
                    x.PenerimaanUnitId,
                    x.CreateDateTime,
                    x.CreateBy,
                    x.CreateByName,
                    x.UnitId,
                    //x.JenisPermintaan,
                    x.TglPenerimaan,
                    x.StatusPenerimaan,
                    x.Keterangan
                })
                .Select(g => new
                {
                    g.Key.PenerimaanUnitId,
                    g.Key.CreateDateTime,
                    g.Key.CreateBy,
                    g.Key.CreateByName,
                    g.Key.UnitId,
                    //g.Key.JenisPermintaan,
                    g.Key.TglPenerimaan,
                    g.Key.StatusPenerimaan,
                    g.Key.Keterangan,

                    DetailPermintaan = g.Where(x => x.Detail != null)
                                        .Select(x => x.Detail)
                                        .ToList()
                })
                .OrderByDescending(x => x.CreateDateTime)
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            if (!listdata.Any())
            {
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });
            }

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
        public async Task<IActionResult> GetById(Guid id)
        {
            var query = from a in _applicationDbContext.PenerimaanUnits
                        join u in _applicationDbContext.UserActives
                            on a.CreateBy equals u.UserActiveId into ua
                        from u in ua.DefaultIfEmpty()

                        join d in _applicationDbContext.DetailPenerimaanUnits
                            on a.PenerimaanUnitId equals d.PenerimaanUnitId into detailJoin
                        from d in detailJoin.DefaultIfEmpty()

                        join ob in _applicationDbContext.Obats
                            on d.ObatId equals ob.ObatId into obatJoin
                        from ob in obatJoin.DefaultIfEmpty()

                        join b in _applicationDbContext.BentukObats
                            on ob.BentukObatId equals b.BentukObatId into bentukJoin
                        from b in bentukJoin.DefaultIfEmpty()

                        where (a.IsDelete == false || a.IsDelete == null)
                              && a.PenerimaanUnitId == id
                        select new
                        {
                            a.PenerimaanUnitId,
                            a.CreateDateTime,
                            a.CreateBy,
                            CreateByName = u.FullName,
                            a.UnitId,
                            a.TglPenerimaan,
                            a.StatusPenerimaan,
                            a.Keterangan,

                            Detail = d == null ? null : new
                            {
                                d.DetailPenerimaanUnitId,
                                d.ObatId,
                                NamaObat = ob != null ? ob.ObatName : null,
                                Bentuk = b != null ? b.NamaBentukObat : null,
                                StokObat = ob != null ? ob.Stock : 0,
                                StockMinimal = ob != null ? ob.Minimal : 0,
                                StockMaksimal = ob != null ? ob.Maximal : 0,
                                Dosis = ob != null ? ob.TakaranDosis : 0,
                                HTE = ob != null ? ob.HTEPrice : 0,
                                d.QtyPermintaan,
                                d.SatuanItem,
                                d.KategoriItem,
                                d.Keterangan
                            }
                        };

            var data = await query
                .GroupBy(x => new
                {
                    x.PenerimaanUnitId,
                    x.CreateDateTime,
                    x.CreateBy,
                    x.CreateByName,
                    x.UnitId,
                    x.TglPenerimaan,
                    x.StatusPenerimaan,
                    x.Keterangan
                })
                .Select(g => new
                {
                    g.Key.PenerimaanUnitId,
                    g.Key.CreateDateTime,
                    g.Key.CreateBy,
                    g.Key.CreateByName,
                    g.Key.UnitId,
                    g.Key.TglPenerimaan,
                    g.Key.StatusPenerimaan,
                    g.Key.Keterangan,

                    DetailPermintaan = g.Where(x => x.Detail != null)
                                        .Select(x => x.Detail)
                                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(new { message = "Data tidak ditemukan." });
            }

            return Ok(new
            {
                message = "Ditemukan || 200 OK",
                data
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PenerimaanUnitViewModel vm)
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

                // **Buat Data Baru untuk PermintaanUnit**
                var penerimaanUnitId = Guid.NewGuid();
                var data = new PenerimaanUnit
                {
                    PenerimaanUnitId = penerimaanUnitId,
                    UnitId = vm.UnitId,
                    //JenisPermintaan = vm.JenisPermintaan,
                    TglPenerimaan = TryParseTanggalToUtc(vm.TglPenerimaan),
                    StatusPenerimaan = vm.StatusPenerimaan,
                    Keterangan = vm.Keterangan,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                // **Buat Data Baru untuk DetailPenerimaanUnit**
                var detailItems = new List<DetailPenerimaanUnit>();
                if (vm.DetailPenerimaanUnit != null && vm.DetailPenerimaanUnit.Any())
                {
                    foreach (var detailVm in vm.DetailPenerimaanUnit)
                    {
                        detailItems.Add(new DetailPenerimaanUnit
                        {
                            DetailPenerimaanUnitId = Guid.NewGuid(),
                            PenerimaanUnitId = penerimaanUnitId, // Link to the main record
                            ObatId = detailVm.ObatId,
                            QtyPermintaan = detailVm.QtyPermintaan,
                            SatuanItem = detailVm.SatuanItem,
                            KategoriItem = detailVm.KategoriItem,
                            Keterangan = detailVm.Keterangan,
                            CreateBy = userActiveId,
                            CreateDateTime = DateTimeOffset.UtcNow,
                        });
                    }
                }

                // **Simpan ke Database**
                await _applicationDbContext.PenerimaanUnits.AddAsync(data);
                await _applicationDbContext.DetailPenerimaanUnits.AddRangeAsync(detailItems);

                int result = await _applicationDbContext.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("PenerimaanUnitCreated", new
                {
                    Action = "create",
                    PenerimaanUnitId = data.PenerimaanUnitId
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
        public async Task<IActionResult> Update(Guid id, [FromBody] PenerimaanUnitViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                if (!_applicationDbContext.Database.CanConnect())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // **Ambil user aktif dari JWT**
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);

                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // **Cek data existing**
                var penerimaan = await _applicationDbContext.PenerimaanUnits
                    .FirstOrDefaultAsync(p => p.PenerimaanUnitId == id && (p.IsDelete == false || p.IsDelete == null));

                if (penerimaan == null)
                    return NotFound(new { message = "Data tidak ditemukan." });

                // **Update field utama**
                penerimaan.UnitId = vm.UnitId;
                penerimaan.TglPenerimaan = TryParseTanggalToUtc(vm.TglPenerimaan);
                penerimaan.StatusPenerimaan = vm.StatusPenerimaan;
                penerimaan.Keterangan = vm.Keterangan;
                penerimaan.UpdateBy = userActiveId;
                penerimaan.UpdateDateTime = DateTimeOffset.UtcNow;

                // **Hapus detail lama**
                var oldDetails = await _applicationDbContext.DetailPenerimaanUnits
                    .Where(d => d.PenerimaanUnitId == id)
                    .ToListAsync();

                if (oldDetails.Any())
                    _applicationDbContext.DetailPenerimaanUnits.RemoveRange(oldDetails);

                // **Tambah detail baru**
                var newDetails = new List<DetailPenerimaanUnit>();
                if (vm.DetailPenerimaanUnit != null && vm.DetailPenerimaanUnit.Any())
                {
                    foreach (var detailVm in vm.DetailPenerimaanUnit)
                    {
                        newDetails.Add(new DetailPenerimaanUnit
                        {
                            DetailPenerimaanUnitId = Guid.NewGuid(),
                            PenerimaanUnitId = id,
                            ObatId = detailVm.ObatId,
                            QtyPermintaan = detailVm.QtyPermintaan,
                            SatuanItem = detailVm.SatuanItem,
                            KategoriItem = detailVm.KategoriItem,
                            Keterangan = detailVm.Keterangan,
                            CreateBy = userActiveId,
                            CreateDateTime = DateTimeOffset.UtcNow
                        });
                    }
                }

                if (newDetails.Any())
                    await _applicationDbContext.DetailPenerimaanUnits.AddRangeAsync(newDetails);

                // **Simpan perubahan**
                int result = await _applicationDbContext.SaveChangesAsync();

                await _hubContext.Clients.All.SendAsync("PenerimaanUnitUpdated", new
                {
                    Action = "update",
                    PenerimaanUnitId = penerimaan.PenerimaanUnitId
                });

                if (result > 0)
                    return Ok(new { message = "Update Data Berhasil || 200 OK" });

                return StatusCode(500, new { message = "Data tidak berhasil diupdate ke database." });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal update data: {dbEx.InnerException?.Message}" });
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

                // **Cari Data PenerimaanUnit**
                var penerimaan = await _applicationDbContext.PenerimaanUnits
                    .FirstOrDefaultAsync(p => p.PenerimaanUnitId == id && (p.IsDelete == false || p.IsDelete == null));

                if (penerimaan == null)
                {
                    return NotFound(new { message = "Data penerimaan unit tidak ditemukan." });
                }

                // **Soft Delete Master**
                penerimaan.IsDelete = true;
                penerimaan.DeleteBy = userActiveId;
                penerimaan.DeleteDateTime = DateTimeOffset.UtcNow;
                _applicationDbContext.PenerimaanUnits.Update(penerimaan);

                // **Soft Delete Detail**
                var details = await _applicationDbContext.DetailPenerimaanUnits
                    .Where(d => d.PenerimaanUnitId == id && (d.IsDelete == false || d.IsDelete == null))
                    .ToListAsync();

                if (details.Any())
                {
                    foreach (var d in details)
                    {
                        d.IsDelete = true;
                        d.DeleteBy = userActiveId;
                        d.DeleteDateTime = DateTimeOffset.UtcNow;
                    }
                    _applicationDbContext.DetailPenerimaanUnits.UpdateRange(details);
                }

                // **Simpan perubahan**
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
        public async Task<IActionResult> Paged(
        int page = 1,
        int perPage = 10,
        string? search = null,
        string? orderBy = "CreateDateTime",
        string? sortDirection = "desc",
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                DateTime? startDate = null,
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                DateTime? endDate = null,
        [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var baseQuery = from a in _applicationDbContext.PenerimaanUnits
                        join u in _applicationDbContext.UserActives
                            on a.CreateBy equals u.UserActiveId into ua
                        from u in ua.DefaultIfEmpty()

                            // GroupJoin ke DetailPenerimaanUnits
                        join d in _applicationDbContext.DetailPenerimaanUnits
                            on a.PenerimaanUnitId equals d.PenerimaanUnitId into detailJoin
                        from d in detailJoin.DefaultIfEmpty()

                            // Left join ke Obats
                        join ob in _applicationDbContext.Obats
                            on d.ObatId equals ob.ObatId into obatJoin
                        from ob in obatJoin.DefaultIfEmpty()

                            // Left join ke BentukObats
                        join b in _applicationDbContext.BentukObats
                            on ob.BentukObatId equals b.BentukObatId into bentukJoin
                        from b in bentukJoin.DefaultIfEmpty()

                        where a.IsDelete == false || a.IsDelete == null
                        select new
                        {
                            a.CreateDateTime,
                            a.CreateBy,
                            CreateByName = u.FullName,
                            a.PenerimaanUnitId,
                            a.UnitId,
                            //a.JenisPermintaan,
                            a.TglPenerimaan,
                            a.StatusPenerimaan,
                            a.Keterangan,

                            Detail = d == null ? null : new
                            {
                                d.DetailPenerimaanUnitId,
                                d.ObatId,
                                NamaObat = ob != null ? ob.ObatName : null,
                                Bentuk = b != null ? b.NamaBentukObat : null,
                                StokObat = ob != null ? ob.Stock : 0,
                                StockMinimal = ob != null ? ob.Minimal : 0,
                                StockMaksimal = ob != null ? ob.Maximal : 0,
                                Dosis = ob != null ? ob.TakaranDosis : 0,
                                HTE = ob != null ? ob.HTEPrice : 0,
                                d.QtyPermintaan,
                                d.SatuanItem,
                                d.KategoriItem,
                                d.Keterangan
                            }
                        };

            // 🔎 Filter tanggal
            if (startDate.HasValue && endDate.HasValue)
            {
                var startUtc = startDate.Value.Date.ToUniversalTime();
                var endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
                baseQuery = baseQuery.Where(u =>
                    u.CreateDateTime >= startUtc &&
                    u.CreateDateTime <= endUtc);
            }

            // 🔎 Filter periode
            if (periode.HasValue)
            {
                var today = DateTime.UtcNow.Date;
                switch (periode)
                {
                    case PeriodeFilter.Today:
                        baseQuery = baseQuery.Where(u => u.CreateDateTime.Date == today);
                        break;
                    case PeriodeFilter.ThisWeek:
                        var startWeek = today.AddDays(-(int)today.DayOfWeek);
                        baseQuery = baseQuery.Where(u => u.CreateDateTime.Date >= startWeek && u.CreateDateTime.Date <= today);
                        break;
                    case PeriodeFilter.LastWeek:
                        var lastWeekStart = today.AddDays(-7 - (int)today.DayOfWeek);
                        var lastWeekEnd = lastWeekStart.AddDays(6);
                        baseQuery = baseQuery.Where(u => u.CreateDateTime.Date >= lastWeekStart && u.CreateDateTime.Date <= lastWeekEnd);
                        break;
                    case PeriodeFilter.ThisMonth:
                        baseQuery = baseQuery.Where(u => u.CreateDateTime.Month == today.Month && u.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.LastMonth:
                        var lastMonth = today.AddMonths(-1);
                        baseQuery = baseQuery.Where(u => u.CreateDateTime.Month == lastMonth.Month && u.CreateDateTime.Year == lastMonth.Year);
                        break;
                    case PeriodeFilter.ThisYear:
                        baseQuery = baseQuery.Where(u => u.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.LastYear:
                        baseQuery = baseQuery.Where(u => u.CreateDateTime.Year == today.Year - 1);
                        break;
                    case PeriodeFilter.Last3Months:
                        baseQuery = baseQuery.Where(u => u.CreateDateTime >= today.AddMonths(-3));
                        break;
                    case PeriodeFilter.Last6Months:
                        baseQuery = baseQuery.Where(u => u.CreateDateTime >= today.AddMonths(-6));
                        break;
                }
            }

            // 🔎 Sorting
            baseQuery = sortDirection?.ToLower() == "desc"
                ? orderBy switch
                {
                    "CreateByName" => baseQuery.OrderByDescending(u => u.CreateByName),
                    "CreateDateTime" => baseQuery.OrderByDescending(u => u.CreateDateTime),
                    _ => baseQuery.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateByName" => baseQuery.OrderBy(u => u.CreateByName),
                    "CreateDateTime" => baseQuery.OrderBy(u => u.CreateDateTime),
                    _ => baseQuery.OrderBy(u => u.CreateDateTime)
                };

            // 🔎 Grouping biar DetailPermintaan jadi list
            var groupedQuery = baseQuery
                .GroupBy(x => new
                {
                    x.PenerimaanUnitId,
                    x.CreateDateTime,
                    x.CreateBy,
                    x.CreateByName,
                    x.UnitId,
                    //x.JenisPermintaan,
                    x.TglPenerimaan,
                    x.StatusPenerimaan,
                    x.Keterangan
                })
                .Select(g => new
                {
                    g.Key.PenerimaanUnitId,
                    g.Key.CreateDateTime,
                    g.Key.CreateBy,
                    g.Key.CreateByName,
                    g.Key.UnitId,
                    //g.Key.JenisPermintaan,
                    g.Key.TglPenerimaan,
                    g.Key.StatusPenerimaan,
                    g.Key.Keterangan,
                    DetailPermintaan = g.Where(x => x.Detail != null).Select(x => x.Detail).ToList()
                });

            var totalRows = await groupedQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var rows = await groupedQuery
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            if (!rows.Any())
            {
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });
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
