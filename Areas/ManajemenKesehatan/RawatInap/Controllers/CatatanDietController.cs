using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class CatatanDietController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<CatatanDietController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CatatanDietController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<CatatanDietController> logger,
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

            // Ambil semua CatatanDiet + UserActive
            var catatanQuery = from cd in _applicationDbContext.CatatanDiets
                               join u in _applicationDbContext.UserActives
                                   on cd.CreateBy equals u.UserActiveId into userJoin
                               from u in userJoin.DefaultIfEmpty()
                               where cd.IsDelete == false || cd.IsDelete == null
                               select new
                               {
                                   cd.CatatanDietId,
                                   cd.KunjunganId,
                                   cd.PasienId,
                                   cd.Diet,
                                   cd.StatusDiet,
                                   cd.Keterangan,
                                   cd.Diagnosa,
                                   cd.TglCatatanDiet,
                                   cd.CreateDateTime,
                                   cd.CreateBy,
                                   CreateByName = u.FullName
                               };

            // Hitung total sebelum paging
            var totalRows = await catatanQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            // Paging CatatanDiet
            var catatanList = await catatanQuery
                .OrderByDescending(c => c.CreateDateTime)
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            if (!catatanList.Any())
            {
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });
            }

            // Grouping di memory
            var data = catatanList.Select(c => new
            {
                c.CatatanDietId,
                c.KunjunganId,
                c.PasienId,
                c.Diet,
                c.StatusDiet,
                c.Diagnosa,
                c.Keterangan,
                c.TglCatatanDiet,
                c.CreateDateTime,
                c.CreateBy,
                c.CreateByName,

            });

            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data,
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
            // ✅ Ambil data utama CatatanDiet + UserActive
            var catatan = await (from cd in _applicationDbContext.CatatanDiets
                                 join u in _applicationDbContext.UserActives
                                     on cd.CreateBy equals u.UserActiveId into userJoin
                                 from u in userJoin.DefaultIfEmpty()
                                 where cd.CatatanDietId == id && (cd.IsDelete == false || cd.IsDelete == null)
                                 select new
                                 {
                                     cd.CatatanDietId,
                                     cd.KunjunganId,
                                     cd.PasienId,
                                     cd.Diet,
                                     cd.Diagnosa,
                                     cd.StatusDiet,
                                     cd.Keterangan,
                                     cd.TglCatatanDiet,
                                     cd.CreateDateTime,
                                     cd.CreateBy,
                                     CreateByName = u.FullName
                                 }).FirstOrDefaultAsync();

            if (catatan == null)
            {
                return NotFound(new { message = "Data Catatan Diet tidak ditemukan. || 404 Not Found" });
            }


            // ✅ Format response
            var result = new
            {
                catatan.CatatanDietId,
                catatan.KunjunganId,
                catatan.PasienId,
                catatan.Diet,
                catatan.StatusDiet,
                catatan.Keterangan,
                catatan.Diagnosa,
                catatan.TglCatatanDiet,
                catatan.CreateDateTime,
                catatan.CreateBy,
                catatan.CreateByName,
            };

            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data = result
            });
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CatatanDietViewModel vm)
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

                // ✅ Ambil User ID dari JWT Claims
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

                // ✅ Buat Data CatatanDiet
                var catatanDietId = Guid.NewGuid();
                var data = new CatatanDiet
                {
                    CatatanDietId = catatanDietId,
                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,
                    Diet = vm.Diet,
                    StatusDiet = vm.StatusDiet,
                    Diagnosa = vm.Diagnosa,
                    Keterangan = vm.Keterangan,
                    TglCatatanDiet = TryParseTanggalToUtc(vm.TglCatatanDiet),
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                _applicationDbContext.CatatanDiets.Add(data);

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
        public async Task<IActionResult> Update(Guid id, [FromBody] CatatanDietViewModel vm)
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

                // ✅ Ambil User ID dari JWT Claims
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

                // ✅ Cari data CatatanDiet yang akan diupdate
                var data = await _applicationDbContext.CatatanDiets
                    .FirstOrDefaultAsync(cd => cd.CatatanDietId == id && (cd.IsDelete == false || cd.IsDelete == null));

                if (data == null)
                {
                    return NotFound(new { message = "Data CatatanDiet tidak ditemukan." });
                }

                // ✅ Update field utama
                data.KunjunganId = vm.KunjunganId;
                data.PasienId = vm.PasienId;
                data.Diet = vm.Diet;
                data.StatusDiet = vm.StatusDiet;
                data.Diagnosa = vm.Diagnosa;
                data.Keterangan = vm.Keterangan;
                data.TglCatatanDiet = TryParseTanggalToUtc(vm.TglCatatanDiet);

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Update Data Berhasil || 200 OK" });
                }
                else
                {
                    return StatusCode(500, new { message = "Data tidak berhasil diperbarui ke database." });
                }
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

                // **Cari Data**
                var data = await _applicationDbContext.CatatanDiets.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.CatatanDiets.Update(data);
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

            // 🔹 Query utama CatatanDiet + UserActive
            var query = from cd in _applicationDbContext.CatatanDiets
                        join u in _applicationDbContext.UserActives
                            on cd.CreateBy equals u.UserActiveId into userJoin
                        from u in userJoin.DefaultIfEmpty()
                        where cd.IsDelete == false || cd.IsDelete == null
                        select new
                        {
                            cd.CatatanDietId,
                            cd.KunjunganId,
                            cd.PasienId,
                            cd.Diet,
                            cd.Diagnosa,
                            cd.StatusDiet,
                            cd.Keterangan,
                            cd.TglCatatanDiet,
                            cd.CreateDateTime,
                            cd.CreateBy,
                            CreateByName = u.FullName
                        };

            // 🔹 Filter pencarian umum
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(cd =>
                    EF.Functions.ILike(cd.Diet, $"%{search}%") ||
                    EF.Functions.ILike(cd.StatusDiet, $"%{search}%") ||
                    EF.Functions.ILike(cd.Keterangan, $"%{search}%") ||
                    EF.Functions.ILike(cd.CreateByName, $"%{search}%"));
            }

            // 🔹 Filter tanggal
            if (startDate.HasValue && endDate.HasValue)
            {
                DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                query = query.Where(cd =>
                    cd.CreateDateTime >= startUtc && cd.CreateDateTime <= endUtc);
            }

            // 🔹 Filter periode (Today, ThisWeek, LastMonth, etc.)
            if (periode.HasValue)
            {
                DateTime today = DateTime.UtcNow.Date;

                switch (periode)
                {
                    case PeriodeFilter.Today:
                        query = query.Where(u => u.CreateDateTime.Date == today);
                        break;
                    case PeriodeFilter.ThisWeek:
                        query = query.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-(int)today.DayOfWeek) &&
                            u.CreateDateTime.Date <= today);
                        break;
                    case PeriodeFilter.LastWeek:
                        query = query.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                            u.CreateDateTime.Date < today.AddDays(-(int)today.DayOfWeek));
                        break;
                    case PeriodeFilter.ThisMonth:
                        query = query.Where(u =>
                            u.CreateDateTime.Month == today.Month && u.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.LastMonth:
                        query = query.Where(u =>
                            u.CreateDateTime.Month == today.AddMonths(-1).Month && u.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.ThisYear:
                        query = query.Where(u => u.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.LastYear:
                        query = query.Where(u => u.CreateDateTime.Year == today.Year - 1);
                        break;
                    case PeriodeFilter.Last3Months:
                        query = query.Where(u => u.CreateDateTime >= today.AddMonths(-3));
                        break;
                    case PeriodeFilter.Last6Months:
                        query = query.Where(u => u.CreateDateTime >= today.AddMonths(-6));
                        break;
                }
            }

            // 🔹 Sorting dinamis
            query = sortDirection?.ToLower() == "desc"
                ? orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                    "Diet" => query.OrderByDescending(u => u.Diet),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderBy(u => u.CreateDateTime),
                    "CreateByName" => query.OrderBy(u => u.CreateByName),
                    "Diet" => query.OrderBy(u => u.Diet),
                    _ => query.OrderBy(u => u.CreateDateTime)
                };

            // 🔹 Hitung total sebelum pagination
            var totalRows = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            // 🔹 Ambil data sesuai page
            var catatanList = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            if (!catatanList.Any())
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });


            // 🔹 Gabungkan hasil (grouping di memory)
            var data = catatanList.Select(cd => new
            {
                cd.CatatanDietId,
                cd.KunjunganId,
                cd.PasienId,
                cd.Diet,
                cd.StatusDiet,
                cd.Diagnosa,
                cd.Keterangan,
                cd.TglCatatanDiet,
                cd.CreateDateTime,
                cd.CreateBy,
                cd.CreateByName,
            });

            // 🔹 Return hasil
            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data = new
                {
                    Rows = data,
                    TotalRows = totalRows,
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalPages = totalPages
                }
            });
        }



    }
}
