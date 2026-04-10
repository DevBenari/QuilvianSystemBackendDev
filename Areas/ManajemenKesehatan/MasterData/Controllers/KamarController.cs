using System.Security.Claims;
using System.Text.RegularExpressions;
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
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class KamarController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<KamarController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public KamarController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<KamarController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        private string GetKodeSingkatan(string namaKamar)
        {
            if (string.IsNullOrWhiteSpace(namaKamar)) return "UNK";

            var words = Regex.Matches(namaKamar.Trim(), @"\b\w")
                             .Select(m => m.Value.ToUpper())
                             .Take(3);

            return string.Concat(words);
        }


        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = (from a in _applicationDbContext.Kamars
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.KamarId,
                             a.KelasId,
                             a.KodeKamar,
                             a.NamaKamar,
                             a.TarifHarian,
                             a.Lantai,
                             a.PosisiRuangan,
                             a.Deskripsi,
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
        public async Task<IActionResult> GetById(Guid id)
        {
            var listdata = _applicationDbContext.Kamars.Find(id);
            if (listdata == null)
            {
                return NotFound(new { message = "Data tidak ditemukan." });
            }

            return Ok(new
            {
                message = "Ditemukan || 200 OK",
                data = listdata
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] KamarViewModel vm)
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
                bool isDuplicate = await _applicationDbContext.Kamars
                                    .AnyAsync(c => c.NamaKamar.ToLower().Trim() == vm.NamaKamar.ToLower().Trim() && c.IsDelete == false);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Nama benefit ini telah tersedia" });
                }



                // **Buat Data Baru**
                var data = new Kamar
                {
                    KamarId = Guid.NewGuid(),
                    KelasId = vm.KelasId,
                    KodeKamar = GetKodeSingkatan(vm.NamaKamar),
                    NamaKamar = vm.NamaKamar,
                    TarifHarian = vm.TarifHarian,
                    Lantai = vm.Lantai,
                    PosisiRuangan = vm.PosisiRuangan,
                    Deskripsi = vm.Deskripsi,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                // **Simpan ke Database**
                _applicationDbContext.Kamars.Add(data);
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
        public async Task<IActionResult> Update(Guid id, [FromBody] KamarViewModel vm)
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
                var data = await _applicationDbContext.Kamars.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                //// **Cek Duplikasi**
                bool isDuplicate = await _applicationDbContext.Kamars
                                    .AnyAsync(c => c.NamaKamar.ToLower().Trim() == vm.NamaKamar.ToLower().Trim() && c.IsDelete == false 
                                    && c.KamarId != id);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Nama benefit ini telah tersedia" });
                }

                // **Update Data**
                data.KelasId = vm.KelasId;
                data.KodeKamar = GetKodeSingkatan(vm.NamaKamar);
                data.NamaKamar = vm.NamaKamar;
                data.TarifHarian = vm.TarifHarian;
                data.Lantai = vm.Lantai;
                data.PosisiRuangan = vm.PosisiRuangan;
                data.Deskripsi = vm.Deskripsi;
                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.Kamars.Update(data);
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
                var data = await _applicationDbContext.Kamars.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.Kamars.Update(data);
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
            string? nama = null,
            Guid? kelasid = null,
            Guid? asuransiId = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
    DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
    DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))]
    PeriodeFilter? periode = null)
        {
            page = page < 1 ? 1 : page;
            perPage = perPage < 1 ? 10 : perPage;
            perPage = perPage > 200 ? 200 : perPage;

            orderBy = string.IsNullOrWhiteSpace(orderBy) ? "CreateDateTime" : orderBy.Trim();
            sortDirection = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase) ? "asc" : "desc";

            // =========================
            // 1) BASE QUERY KAMAR
            // =========================
            var query =
                from a in _applicationDbContext.Kamars.AsNoTracking()
                join u0 in _applicationDbContext.UserActives.AsNoTracking()
                    on a.CreateBy equals u0.UserActiveId into uGroup
                from u in uGroup.DefaultIfEmpty()

                    // FIX: join kelas harus a.KelasId == kl.KelasId
                join kl0 in _applicationDbContext.Kelass.AsNoTracking()
                    on a.KelasId equals kl0.KelasId into klGroup
                from kl in klGroup.DefaultIfEmpty()

                where a.IsDelete == false || a.IsDelete == null
                select new
                {
                    a.CreateDateTime,
                    a.CreateBy,
                    CreateByName = u != null ? u.FullName : null,
                    a.KamarId,
                    a.KelasId,
                    NamaKelas = kl != null ? kl.NamaKelas : null,
                    a.KodeKamar,
                    a.NamaKamar,
                    a.TarifHarian,
                    a.Lantai,
                    a.PosisiRuangan,
                    a.Deskripsi
                };

            // =========================
            // 2) FILTER
            // =========================

            if (!string.IsNullOrWhiteSpace(nama))
            {
                var keyword = $"%{nama.Trim()}%";
                query = query.Where(x => EF.Functions.ILike(x.NamaKamar, keyword));
            }

            if (kelasid.HasValue)
            {
                query = query.Where(x => x.KelasId == kelasid.Value);
            }

            // Filter by AsuransiId
            // Aman untuk nullable KamarAsuransi.KamarId / AsuransiId
            if (asuransiId.HasValue)
            {
                var aid = asuransiId.Value;

                query = query.Where(x =>
                    _applicationDbContext.KamarAsuransis.AsNoTracking().Any(ka =>
                        (ka.IsDelete == false || ka.IsDelete == null) &&
                        ka.KamarId.HasValue &&
                        ka.KamarId.Value == x.KamarId &&
                        ka.AsuransiId.HasValue &&
                        ka.AsuransiId.Value == aid
                    ));
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                var startUtc = new DateTimeOffset(startDate.Value.Date, TimeSpan.Zero);
                var endUtcExclusive = new DateTimeOffset(endDate.Value.Date.AddDays(1), TimeSpan.Zero);

                query = query.Where(x =>
                    x.CreateDateTime >= startUtc &&
                    x.CreateDateTime < endUtcExclusive);
            }

            if (periode.HasValue)
            {
                var today = DateTime.UtcNow.Date;
                DateTime? rangeStart = null;
                DateTime? rangeEndExclusive = null;

                switch (periode.Value)
                {
                    case PeriodeFilter.Today:
                        rangeStart = today;
                        rangeEndExclusive = today.AddDays(1);
                        break;

                    case PeriodeFilter.ThisWeek:
                        {
                            int diff = (7 + ((int)today.DayOfWeek == 0 ? 7 : (int)today.DayOfWeek) - (int)DayOfWeek.Monday) % 7;
                            var startWeek = today.AddDays(-diff);
                            rangeStart = startWeek;
                            rangeEndExclusive = today.AddDays(1);
                            break;
                        }

                    case PeriodeFilter.LastWeek:
                        {
                            int diff = (7 + ((int)today.DayOfWeek == 0 ? 7 : (int)today.DayOfWeek) - (int)DayOfWeek.Monday) % 7;
                            var thisWeekStart = today.AddDays(-diff);
                            rangeStart = thisWeekStart.AddDays(-7);
                            rangeEndExclusive = thisWeekStart;
                            break;
                        }

                    case PeriodeFilter.ThisMonth:
                        {
                            var startMonth = new DateTime(today.Year, today.Month, 1);
                            rangeStart = startMonth;
                            rangeEndExclusive = startMonth.AddMonths(1);
                            break;
                        }

                    case PeriodeFilter.LastMonth:
                        {
                            var lastMonth = today.AddMonths(-1);
                            var startLastMonth = new DateTime(lastMonth.Year, lastMonth.Month, 1);
                            rangeStart = startLastMonth;
                            rangeEndExclusive = startLastMonth.AddMonths(1);
                            break;
                        }

                    case PeriodeFilter.ThisYear:
                        {
                            var startYear = new DateTime(today.Year, 1, 1);
                            rangeStart = startYear;
                            rangeEndExclusive = startYear.AddYears(1);
                            break;
                        }

                    case PeriodeFilter.LastYear:
                        {
                            var startLastYear = new DateTime(today.Year - 1, 1, 1);
                            rangeStart = startLastYear;
                            rangeEndExclusive = startLastYear.AddYears(1);
                            break;
                        }

                    case PeriodeFilter.Last3Months:
                        rangeStart = today.AddMonths(-3);
                        rangeEndExclusive = today.AddDays(1);
                        break;

                    case PeriodeFilter.Last6Months:
                        rangeStart = today.AddMonths(-6);
                        rangeEndExclusive = today.AddDays(1);
                        break;
                }

                if (rangeStart.HasValue && rangeEndExclusive.HasValue)
                {
                    var start = new DateTimeOffset(rangeStart.Value, TimeSpan.Zero);
                    var endEx = new DateTimeOffset(rangeEndExclusive.Value, TimeSpan.Zero);

                    query = query.Where(x =>
                        x.CreateDateTime >= start &&
                        x.CreateDateTime < endEx);
                }
            }

            // =========================
            // 3) SORTING
            // =========================
            query = (orderBy, sortDirection) switch
            {
                ("CreateDateTime", "asc") => query.OrderBy(x => x.CreateDateTime).ThenBy(x => x.KamarId),
                _ => query.OrderByDescending(x => x.CreateDateTime).ThenByDescending(x => x.KamarId)
            };

            // =========================
            // 4) COUNT
            // =========================
            var totalRows = await query.CountAsync();
            var totalPages = totalRows == 0 ? 0 : (int)Math.Ceiling(totalRows / (double)perPage);

            if (totalRows == 0)
            {
                return Ok(new
                {
                    status = "success",
                    message = "Data retrieved successfully",
                    data = new
                    {
                        Rows = new List<object>(),
                        TotalRows = 0,
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalPages = 0
                    }
                });
            }

            if (page > totalPages)
            {
                return NotFound(new { message = "Page not found." });
            }

            // =========================
            // 5) PAGED ROWS
            // =========================
            var rows = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            // =========================
            // 6) LOAD GROUPED KamarAsuransi
            //    hanya untuk kamar di halaman ini
            // =========================
            var kamarIds = rows.Select(x => x.KamarId).Distinct().ToList();

            var kamarAsuransiRaw = await (
                from ka in _applicationDbContext.KamarAsuransis.AsNoTracking()
                join a0 in _applicationDbContext.Asuransis.AsNoTracking()
                    on ka.AsuransiId equals a0.AsuransiId into aGroup
                from a in aGroup.DefaultIfEmpty()
                where (ka.IsDelete == false || ka.IsDelete == null)
                      && ka.KamarId.HasValue
                      && kamarIds.Contains(ka.KamarId.Value)
                select new
                {
                    KamarId = ka.KamarId!.Value,
                    AsuransiId = ka.AsuransiId,
                    NamaAsuransi = a != null ? a.NamaAsuransi : null
                }
            ).ToListAsync();

            var kamarAsuransiLookup = kamarAsuransiRaw
                .GroupBy(x => x.KamarId)
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        TotalAsuransi = g
                            .Where(x => x.AsuransiId.HasValue)
                            .Select(x => x.AsuransiId!.Value)
                            .Distinct()
                            .Count(),

                        Asuransis = g
                            .Where(x => x.AsuransiId.HasValue)
                            .GroupBy(x => x.AsuransiId!.Value)
                            .Select(ag => (object)new
                            {
                                AsuransiId = ag.Key,
                                NamaAsuransi = ag
                                    .Select(x => x.NamaAsuransi)
                                    .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))
                            })
                            .ToList()
                    });

            // =========================================
            // 7) MERGE FINAL RESULT
            // =========================================
            var finalRows = rows.Select(x =>
            {
                kamarAsuransiLookup.TryGetValue(x.KamarId, out var ka);

                return new
                {
                    x.CreateDateTime,
                    x.CreateBy,
                    x.CreateByName,
                    x.KamarId,
                    x.KelasId,
                    x.NamaKelas,
                    x.KodeKamar,
                    x.NamaKamar,
                    x.TarifHarian,
                    x.Lantai,
                    x.PosisiRuangan,
                    x.Deskripsi,

                    TotalAsuransi = ka?.TotalAsuransi ?? 0,
                    Asuransis = ka?.Asuransis ?? new List<object>()
                };
            }).ToList();
            return Ok(new
            {
                status = "success",
                message = "Data retrieved successfully",
                data = new
                {
                    Rows = finalRows,
                    TotalRows = totalRows,
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalPages = totalPages
                }
            });
        }
    }
}
