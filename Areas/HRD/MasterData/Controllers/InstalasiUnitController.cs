using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.HRD.MasterData.Models;
using QuilvianSystemBackendDev.Areas.HRD.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class InstalasiUnitController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<InstalasiUnitController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public InstalasiUnitController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<InstalasiUnitController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = (from a in _applicationDbContext.InstalasiUnits
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.InstalasiUnitId,
                             a.KodeInstalasiUnit,
                             a.NamaInstalasiUnit,
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
        public async Task<IActionResult> GetById(Guid id)
        {
            var listdata = _applicationDbContext.InstalasiUnits.Find(id);
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
        public async Task<IActionResult> Create([FromBody] InstalasiUnitViewModel vm)
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
                bool isDuplicate = await _applicationDbContext.InstalasiUnits
                                    .AnyAsync(c => c.NamaInstalasiUnit.ToLower().Trim() == vm.NamaInstalasiUnit.ToLower().Trim()
                                    && c.IsDelete == false);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Nama instalasi unit ini telah tersedia" });
                }

                // **Ambil Tanggal Sekarang**
                var dateNow = DateTime.UtcNow;
                var setDateNow = dateNow.ToString("yyMMdd"); // Format: YYMMDD
                var start = dateNow.Date;
                var end = start.AddDays(1);

                // Ambil data terakhir hari ini (lebih aman daripada .Date)
                var lastCode = await _applicationDbContext.InstalasiUnits
                    .AsNoTracking()
                    .Where(x => x.CreateDateTime >= start && x.CreateDateTime < end)
                    .OrderByDescending(x => x.CreateDateTime)
                    .Select(x => x.KodeInstalasiUnit)
                    .FirstOrDefaultAsync();

                string kode;

                if (string.IsNullOrWhiteSpace(lastCode))
                {
                    kode = $"IU{setDateNow}0001";
                }
                else
                {
                    // Pastikan format minimal: "IU" + 8 digit tanggal + 4 digit nomor = 14 char
                    // IU202601130001 (14)
                    if (lastCode.Length < 14 || !lastCode.StartsWith("IU"))
                    {
                        kode = $"IU{setDateNow}0001";
                    }
                    else
                    {
                        var lastDatePart = lastCode.Substring(2, 8);   // yyyyMMdd
                        if (lastDatePart != setDateNow)
                        {
                            kode = $"IU{setDateNow}0001";
                        }
                        else
                        {
                            var lastNumberPart = lastCode.Substring(10, 4); // NNNN
                            if (!int.TryParse(lastNumberPart, out var lastNumber))
                            {
                                kode = $"IU{setDateNow}0001";
                            }
                            else
                            {
                                kode = $"IU{setDateNow}{(lastNumber + 1):D4}";
                            }
                        }
                    }
                }

                // **Buat Data Baru**
                var data = new InstalasiUnit
                {
                    InstalasiUnitId = Guid.NewGuid(),
                    NamaInstalasiUnit = vm.NamaInstalasiUnit,
                    KodeInstalasiUnit = kode,
                    Keterangan = vm.Keterangan,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                // **Simpan ke Database**
                _applicationDbContext.InstalasiUnits.Add(data);
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
        public async Task<IActionResult> Update(Guid id, [FromBody] InstalasiUnitViewModel vm)
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
                var data = await _applicationDbContext.InstalasiUnits.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                //// **Cek Duplikasi**
                bool isDuplicate = await _applicationDbContext.InstalasiUnits
                                    .AnyAsync(c => c.NamaInstalasiUnit.ToLower().Trim() == vm.NamaInstalasiUnit.ToLower().Trim()
                                    && c.IsDelete == false && c.InstalasiUnitId != id);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Nama diskon ini telah tersedia" });
                }

                // **Update Data**
                data.NamaInstalasiUnit = vm.NamaInstalasiUnit;
                data.Keterangan = vm.Keterangan;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.InstalasiUnits.Update(data);
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
                var data = await _applicationDbContext.InstalasiUnits.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.InstalasiUnits.Update(data);
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
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null,

            // ✅ tambahan filter
            Guid? instalasiUnitId = null,
            Guid? departementId = null
        )
        {
            page = page < 1 ? 1 : page;
            perPage = perPage < 1 ? 10 : perPage;
            perPage = Math.Min(perPage, 100);

            // ===== hitung range tanggal (index-friendly) =====
            DateTime? rangeStartUtc = null;
            DateTime? rangeEndExclusiveUtc = null;

            if (startDate.HasValue && endDate.HasValue)
            {
                rangeStartUtc = startDate.Value.Date.ToUniversalTime();
                rangeEndExclusiveUtc = endDate.Value.Date.AddDays(1).ToUniversalTime();
            }
            else if (periode.HasValue)
            {
                var today = DateTime.UtcNow.Date;

                switch (periode.Value)
                {
                    case PeriodeFilter.Today:
                        rangeStartUtc = today; rangeEndExclusiveUtc = today.AddDays(1); break;

                    case PeriodeFilter.ThisWeek:
                        var weekStart = today.AddDays(-(int)today.DayOfWeek);
                        rangeStartUtc = weekStart; rangeEndExclusiveUtc = today.AddDays(1); break;

                    case PeriodeFilter.LastWeek:
                        var thisWeekStart = today.AddDays(-(int)today.DayOfWeek);
                        rangeStartUtc = thisWeekStart.AddDays(-7); rangeEndExclusiveUtc = thisWeekStart; break;

                    case PeriodeFilter.ThisMonth:
                        var monthStart = new DateTime(today.Year, today.Month, 1);
                        rangeStartUtc = monthStart; rangeEndExclusiveUtc = monthStart.AddMonths(1); break;

                    case PeriodeFilter.LastMonth:
                        var thisMonthStart = new DateTime(today.Year, today.Month, 1);
                        var lastMonthStart = thisMonthStart.AddMonths(-1);
                        rangeStartUtc = lastMonthStart; rangeEndExclusiveUtc = thisMonthStart; break;

                    case PeriodeFilter.ThisYear:
                        var yearStart = new DateTime(today.Year, 1, 1);
                        rangeStartUtc = yearStart; rangeEndExclusiveUtc = yearStart.AddYears(1); break;

                    case PeriodeFilter.LastYear:
                        var thisYearStart2 = new DateTime(today.Year, 1, 1);
                        var lastYearStart2 = thisYearStart2.AddYears(-1);
                        rangeStartUtc = lastYearStart2; rangeEndExclusiveUtc = thisYearStart2; break;

                    case PeriodeFilter.Last3Months:
                        rangeStartUtc = today.AddMonths(-3); rangeEndExclusiveUtc = today.AddDays(1); break;

                    case PeriodeFilter.Last6Months:
                        rangeStartUtc = today.AddMonths(-6); rangeEndExclusiveUtc = today.AddDays(1); break;
                }
            }

            // ===== BASE QUERY =====
            var baseQuery =
                from a in _applicationDbContext.InstalasiUnits.AsNoTracking()
                where a.IsDelete == false || a.IsDelete == null

                join d0 in _applicationDbContext.Departements.AsNoTracking()
                    on a.DepartementId equals d0.DepartementId into dJoin
                from d in dJoin.DefaultIfEmpty()

                join u0 in _applicationDbContext.UserActives.AsNoTracking()
                    on a.CreateBy equals u0.UserActiveId into uJoin
                from u in uJoin.DefaultIfEmpty()

                select new
                {
                    a.DepartementId,
                    DepartementName = d != null ? d.NamaDepartement :null,

                    a.CreateDateTime,
                    a.CreateBy,
                    CreateByName = u != null ? u.FullName : null,

                    a.InstalasiUnitId,
                    a.KodeInstalasiUnit,
                    a.NamaInstalasiUnit,
                    a.Keterangan
                };

            // ✅ Filter by InstalasiUnitId (paling cepat karena PK)
            if (instalasiUnitId.HasValue)
                baseQuery = baseQuery.Where(x => x.InstalasiUnitId == instalasiUnitId.Value);

            // ✅ Filter by DepartementId (FK)
            if (departementId.HasValue)
                baseQuery = baseQuery.Where(x => x.DepartementId == departementId.Value);

            // Search teks (nama unit)
            if (!string.IsNullOrWhiteSpace(search))
            {
                var pattern = $"%{search.Trim().ToLower()}%";
                baseQuery = baseQuery.Where(x => EF.Functions.ILike(x.NamaInstalasiUnit, pattern));
            }

            // Filter tanggal range
            if (rangeStartUtc.HasValue && rangeEndExclusiveUtc.HasValue)
            {
                var start = rangeStartUtc.Value;
                var endEx = rangeEndExclusiveUtc.Value;
                baseQuery = baseQuery.Where(x => x.CreateDateTime >= start && x.CreateDateTime < endEx);
            }

            // ===== Tahap A: paging departemen =====
            var deptAgg =
                baseQuery
                    .GroupBy(x => new { x.DepartementId, x.DepartementName })
                    .Select(g => new
                    {
                        g.Key.DepartementId,
                        g.Key.DepartementName,
                        TotalRows = g.Count(),
                        LatestCreateDateTime = g.Max(x => x.CreateDateTime)
                    });

            bool desc = (sortDirection ?? "desc").Equals("desc", StringComparison.OrdinalIgnoreCase);

            deptAgg = (orderBy ?? "CreateDateTime") switch
            {
                "DepartementName" => desc ? deptAgg.OrderByDescending(x => x.DepartementName) : deptAgg.OrderBy(x => x.DepartementName),
                "TotalRows" => desc ? deptAgg.OrderByDescending(x => x.TotalRows) : deptAgg.OrderBy(x => x.TotalRows),
                _ => desc ? deptAgg.OrderByDescending(x => x.LatestCreateDateTime) : deptAgg.OrderBy(x => x.LatestCreateDateTime),
            };

            var totalGroups = await deptAgg.CountAsync();
            var totalPages = (int)Math.Ceiling(totalGroups / (double)perPage);

            if (totalGroups > 0 && page > totalPages)
                return NotFound(new { message = "Page not found." });

            var deptsPage = await deptAgg
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            var deptIds = deptsPage.Select(x => x.DepartementId).ToList();

            // ===== Tahap B: ambil rows hanya untuk departemen pada halaman =====
            var rowsQuery = baseQuery.Where(x => deptIds.Contains(x.DepartementId));

            rowsQuery = (orderBy ?? "CreateDateTime") switch
            {
                "CreateByName" => desc ? rowsQuery.OrderByDescending(x => x.CreateByName) : rowsQuery.OrderBy(x => x.CreateByName),
                "NamaInstalasiUnit" => desc ? rowsQuery.OrderByDescending(x => x.NamaInstalasiUnit) : rowsQuery.OrderBy(x => x.NamaInstalasiUnit),
                _ => desc ? rowsQuery.OrderByDescending(x => x.CreateDateTime) : rowsQuery.OrderBy(x => x.CreateDateTime),
            };

            var rows = await rowsQuery.ToListAsync();

            var groups = deptsPage.Select(d => new
            {
                d.DepartementId,
                d.DepartementName,
                d.TotalRows,
                Rows = rows.Where(r => r.DepartementId == d.DepartementId)
                           .Select(r => new
                           {
                               r.CreateDateTime,
                               r.CreateBy,
                               r.CreateByName,
                               r.InstalasiUnitId,
                               r.KodeInstalasiUnit,
                               r.NamaInstalasiUnit,
                               r.Keterangan
                           })
                           .ToList()
            });

            return Ok(new
            {
                status = "success",
                message = "Data retrieved successfully",
                data = new
                {
                    Groups = groups,
                    TotalGroups = totalGroups,
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalPages = totalPages
                }
            });
        }


    }
}
