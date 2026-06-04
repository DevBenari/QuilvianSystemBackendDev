using System.Globalization;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class TindakanHarianController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<TindakanHarianController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TindakanHarianController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<TindakanHarianController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
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
                    DateTimeKind.Local
                ); // atau Utc jika perlu

                return finalDateTime.ToUniversalTime(); // simpan dalam UTC
            }
            return null;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // 🔹 Ambil data utama (paged)
            var tindakanHariansQuery =
                from a in _applicationDbContext.TindakanHarians
                join u in _applicationDbContext.UserActives on a.CreateBy equals u.UserActiveId into userGroup
                from u in userGroup.DefaultIfEmpty()
                where a.IsDelete == false || a.IsDelete == null
                orderby a.CreateDateTime descending
                select new
                {
                    a.TindakanHarianId,
                    a.TindakanPerawatId,
                    a.KunjunganId,
                    a.PasienId,
                    a.TglTindakanHarian,
                    a.WaktuTindakanHarian,
                    a.ShiftTime,
                    a.NamaPerawat,
                    a.Diagnosa,
                    a.Keterangan,
                    a.CreateDateTime,
                    a.CreateBy,
                    CreateByName = u.FullName
                };

            var totalRows = await tindakanHariansQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var tindakanHarians = await tindakanHariansQuery
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            if (!tindakanHarians.Any())
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });

            // 🔹 Ambil semua ID tindakan perawat unik dari seluruh record
            var allTindakanIds = tindakanHarians
                .Where(x => x.TindakanPerawatId != null)
                .SelectMany(x => x.TindakanPerawatId!)
                .Distinct()
                .ToList();

            // 🔹 Ambil semua data TindakanPerawat yang berhubungan dalam satu query
            var tindakanPerawats = await _applicationDbContext.TindakanPerawats
                .Where(tp => allTindakanIds.Contains((Guid)tp.TindakanPerawatId))
                .ToListAsync();

            // 🔹 Gabungkan hasilnya di memory
            var result = tindakanHarians.Select(a => new
            {
                a.TindakanHarianId,
                a.KunjunganId,
                a.PasienId,
                a.TglTindakanHarian,
                a.WaktuTindakanHarian,
                a.ShiftTime,
                a.Diagnosa,
                a.NamaPerawat,
                a.Keterangan,
                a.CreateDateTime,
                a.CreateBy,
                a.CreateByName,
                DaftarTindakan = tindakanPerawats
                    .Where(tp => a.TindakanPerawatId != null && a.TindakanPerawatId.Contains((Guid)tp.TindakanPerawatId))
                    .Select(tp => new
                    {
                        tp.TindakanPerawatId,
                        tp.NamaTindakanPerawat,
                        tp.KategoriTindakan,
                        tp.Keterangan
                    })
                    .ToList()
            });

            // 🔹 Return hasil
            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data = result,
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
            // 🔹 Ambil data utama
            var tindakanHarian = await (
                from a in _applicationDbContext.TindakanHarians
                join u in _applicationDbContext.UserActives on a.CreateBy equals u.UserActiveId into userGroup
                from u in userGroup.DefaultIfEmpty()
                where a.TindakanHarianId == id && (a.IsDelete == false || a.IsDelete == null)
                select new
                {
                    a.TindakanHarianId,
                    a.TindakanPerawatId,
                    a.KunjunganId,
                    a.PasienId,
                    a.TglTindakanHarian,
                    a.WaktuTindakanHarian,
                    a.ShiftTime,
                    a.NamaPerawat,
                    a.Diagnosa,
                    a.Keterangan,
                    a.CreateDateTime,
                    a.CreateBy,
                    CreateByName = u.FullName
                }
            ).FirstOrDefaultAsync();

            if (tindakanHarian == null)
            {
                return NotFound(new { message = "Data tindakan harian tidak ditemukan. || 404 Not Found" });
            }

            // 🔹 Ambil semua ID tindakan perawat dari list
            var tindakanIds = tindakanHarian.TindakanPerawatId ?? new List<Guid>();

            // 🔹 Ambil data tindakan perawat terkait dalam satu query
            var tindakanPerawats = await _applicationDbContext.TindakanPerawats
                .Where(tp => tindakanIds.Contains((Guid)tp.TindakanPerawatId))
                .Select(tp => new
                {
                    tp.TindakanPerawatId,
                    tp.NamaTindakanPerawat,
                    tp.KategoriTindakan,
                    tp.Keterangan
                })
                .ToListAsync();

            // 🔹 Gabungkan hasil
            var result = new
            {
                tindakanHarian.TindakanHarianId,
                tindakanHarian.KunjunganId,
                tindakanHarian.PasienId,
                tindakanHarian.TglTindakanHarian,
                tindakanHarian.WaktuTindakanHarian,
                tindakanHarian.ShiftTime,
                tindakanHarian.NamaPerawat,
                tindakanHarian.Diagnosa,
                tindakanHarian.Keterangan,
                tindakanHarian.CreateDateTime,
                tindakanHarian.CreateBy,
                tindakanHarian.CreateByName,
                DaftarTindakan = tindakanPerawats
            };

            // 🔹 Return hasil
            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data = result
            });
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TindakanHarianViewModel vm)
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

                //// **Cek Duplikasi**
                //bool isDuplicate = _applicationDbContext.Diskons
                //                    .Any(c => c.NamaDiskon == vm.NamaDiskon);

                //if (isDuplicate)
                //{
                //    return Conflict(new { message = "Nama benefit ini telah tersedia" });
                //}

                // **Buat Data Baru**
                var data = new TindakanHarian
                {
                    TindakanHarianId = Guid.NewGuid(),
                    TindakanPerawatId = vm.TindakanPerawatId,
                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,
                    TglTindakanHarian = TryParseTanggalToUtc(vm.TglTindakanHarian),
                    WaktuTindakanHarian = vm.WaktuTindakanHarian,
                    ShiftTime = vm.ShiftTime,
                    NamaPerawat = vm.NamaPerawat,
                    Diagnosa = vm.Diagnosa,
                    Keterangan = vm.Keterangan,
                    
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                // **Simpan ke Database**
                _applicationDbContext.TindakanHarians.Add(data);
                int result = await _applicationDbContext.SaveChangesAsync();

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
        public async Task<IActionResult> Update(Guid id, [FromBody] TindakanHarianViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

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
                var data = await _applicationDbContext.TindakanHarians.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Update Data**
                data.TindakanPerawatId =  vm.TindakanPerawatId;
                data.KunjunganId = vm.KunjunganId;
                data.PasienId  = vm.PasienId;
                data.TglTindakanHarian = TryParseTanggalToUtc(vm.TglTindakanHarian);
                data.WaktuTindakanHarian = vm.WaktuTindakanHarian;
                data.ShiftTime = vm.ShiftTime;
                data.NamaPerawat = vm.NamaPerawat;
                data.Diagnosa = vm.Diagnosa;
                data.Keterangan = vm.Keterangan;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.TindakanHarians.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Update Data Berhasil || 200 OK" });
                }
                else
                {
                    return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
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
                var data = await _applicationDbContext.TindakanHarians.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.TindakanHarians.Update(data);
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
            [FromQuery] DateTimeOffset? createDate = null,
            string? keterangan = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // ===============================================================
            // 🔹 Step 1 — BaseQuery
            // ===============================================================
            var baseQuery =
                from a in _applicationDbContext.TindakanHarians
                join u in _applicationDbContext.UserActives on a.CreateBy equals u.UserActiveId into userGroup
                from u in userGroup.DefaultIfEmpty()
                where a.IsDelete == false || a.IsDelete == null
                select new
                {
                    a.TindakanHarianId,
                    a.TindakanPerawatId,
                    a.KunjunganId,
                    a.PasienId,
                    a.TglTindakanHarian,
                    a.WaktuTindakanHarian,
                    a.ShiftTime,
                    a.NamaPerawat,
                    a.Diagnosa,
                    a.Keterangan,
                    a.CreateDateTime,
                    a.CreateBy,
                    CreateByName = u.FullName
                };

            // ===============================================================
            // 🔹 Step 2 — Filter tanggal (range)
            // ===============================================================
            if (startDate.HasValue && endDate.HasValue)
            {
                var start = new DateTimeOffset(startDate.Value.Date, TimeSpan.Zero);
                var end = new DateTimeOffset(endDate.Value.Date.AddDays(1).AddTicks(-1), TimeSpan.Zero);

                baseQuery = baseQuery.Where(u =>
                    u.CreateDateTime >= start &&
                    u.CreateDateTime <= end);
            }

            // ===============================================================
            // 🔹 Step 2B — Filter exact CreateDateTime (per hari)
            // ===============================================================
            if (createDate.HasValue)
            {
                var dayStart = createDate.Value.Date;
                var dayEnd = createDate.Value.Date.AddDays(1).AddTicks(-1);

                baseQuery = baseQuery.Where(x =>
                    x.CreateDateTime >= dayStart &&
                    x.CreateDateTime <= dayEnd);
            }

            // ===============================================================
            // 🔹 Step 3 — Filter periode
            // ===============================================================
            if (periode.HasValue)
            {
                var today = DateTime.UtcNow.Date;

                baseQuery = periode switch
                {
                    PeriodeFilter.Today =>
                        baseQuery.Where(u => u.CreateDateTime.Date == today),

                    PeriodeFilter.ThisWeek =>
                        baseQuery.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-(int)today.DayOfWeek) &&
                            u.CreateDateTime.Date <= today),

                    PeriodeFilter.LastWeek =>
                        baseQuery.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                            u.CreateDateTime.Date < today.AddDays(-(int)today.DayOfWeek)),

                    PeriodeFilter.ThisMonth =>
                        baseQuery.Where(u =>
                            u.CreateDateTime.Month == today.Month &&
                            u.CreateDateTime.Year == today.Year),

                    PeriodeFilter.LastMonth =>
                        baseQuery.Where(u =>
                            u.CreateDateTime.Month == today.Month - 1 &&
                            u.CreateDateTime.Year == today.Year),

                    PeriodeFilter.ThisYear =>
                        baseQuery.Where(u => u.CreateDateTime.Year == today.Year),

                    PeriodeFilter.LastYear =>
                        baseQuery.Where(u => u.CreateDateTime.Year == today.Year - 1),

                    PeriodeFilter.Last3Months =>
                        baseQuery.Where(u => u.CreateDateTime >= today.AddMonths(-3)),

                    PeriodeFilter.Last6Months =>
                        baseQuery.Where(u => u.CreateDateTime >= today.AddMonths(-6)),

                    _ => baseQuery
                };
            }

            // ===============================================================
            // 🔹 Step 4 — Filter berdasarkan KunjunganId (HARUS di SQL!)
            // ===============================================================
            if (kunjunganId.HasValue)
            {
                baseQuery = baseQuery.Where(u => u.KunjunganId == kunjunganId.Value);
            }

            // ===============================================================
            // 🔹 Step 5 — Filter berdasarkan keterangan (ILIKE, SQL)
            // ===============================================================
            if (!string.IsNullOrWhiteSpace(keterangan))
            {
                string pattern = $"%{keterangan.Trim()}%";
                baseQuery = baseQuery.Where(u =>
                    EF.Functions.ILike(u.Keterangan ?? "", pattern));
            }

            // ===============================================================
            // 🔹 Step 6 — Sorting
            // ===============================================================
            baseQuery = sortDirection?.ToLower() == "desc"
                ? orderBy switch
                {
                    "CreateByName" => baseQuery.OrderByDescending(u => u.CreateByName),
                    "TglTindakanHarian" => baseQuery.OrderByDescending(u => u.TglTindakanHarian),
                    _ => baseQuery.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateByName" => baseQuery.OrderBy(u => u.CreateByName),
                    "TglTindakanHarian" => baseQuery.OrderBy(u => u.TglTindakanHarian),
                    _ => baseQuery.OrderBy(u => u.CreateDateTime)
                };

            // ===============================================================
            // 🔹 Step 7 — Paging (SQL)
            // ===============================================================
            var totalRows = await baseQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var pagedData = await baseQuery
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            if (!pagedData.Any())
                return NotFound(new { message = "Tidak ada data untuk halaman ini." });

            // ===============================================================
            // 🔹 Step 8 — Load Tindakan Perawat
            // ===============================================================
            var allTindakanIds = pagedData
                .Where(x => x.TindakanPerawatId != null)
                .SelectMany(x => x.TindakanPerawatId!)
                .Distinct()
                .ToList();

            var tindakanPerawats = await _applicationDbContext.TindakanPerawats
                .Where(tp => allTindakanIds.Contains(tp.TindakanPerawatId ?? Guid.Empty))
                .Select(tp => new
                {
                    tp.TindakanPerawatId,
                    tp.NamaTindakanPerawat,
                    tp.KategoriTindakan,
                    tp.Keterangan
                })
                .ToListAsync();

            // ===============================================================
            // 🔹 Step 9 — Merge hasil
            // ===============================================================
            var result = pagedData.Select(a => new
            {
                a.TindakanHarianId,
                a.KunjunganId,
                a.PasienId,
                a.TglTindakanHarian,
                a.WaktuTindakanHarian,
                a.ShiftTime,
                a.NamaPerawat,
                a.Diagnosa,
                a.Keterangan,
                a.CreateDateTime,
                a.CreateBy,
                a.CreateByName,
                DaftarTindakan = tindakanPerawats
                    .Where(tp => a.TindakanPerawatId != null &&
                                 a.TindakanPerawatId.Contains(tp.TindakanPerawatId!.Value))
                    .ToList()
            });

            // ===============================================================
            // 🔹 Step 10 — Return
            // ===============================================================
            return Ok(new
            {
                status = "success",
                message = "Data retrieved successfully",
                data = new
                {
                    Rows = result,
                    TotalRows = totalRows,
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalPages = totalPages
                }
            });
        }




    }
}
