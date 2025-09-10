using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class SOAPPlanningController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<SOAPPlanningController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SOAPPlanningController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<SOAPPlanningController> logger,
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
            try
            {
                if (page < 1) page = 1;
                if (perPage < 1) perPage = 10;

                var baseQuery =
                    from a in _applicationDbContext.SOAPPlannings.AsNoTracking()
                    join u in _applicationDbContext.UserActives.AsNoTracking()
                        on a.CreateBy equals u.UserActiveId into gu
                    from u in gu.DefaultIfEmpty() // LEFT JOIN User
                                                  // >>> GANTI `ICD10Id` DI BAWAH DENGAN NAMA PK GUID DI TABEL ICD10s MILIKMU <<<
                    join icd in _applicationDbContext.ICD10s.AsNoTracking()
                        on a.IcdId equals icd.ICDId into gi
                    from icd in gi.DefaultIfEmpty() // LEFT JOIN ICD10s
                    join plan in _applicationDbContext.ICDPlannings.AsNoTracking()
                        on a.PlanningIcdId equals plan.ICDPlanningId into gp
                    from plan in gp.DefaultIfEmpty() // LEFT JOIN ICDPlannings
                    where a.IsDelete != true
                    select new
                    {
                        a.CreateDateTime,
                        a.CreateBy,
                        CreateByName = u != null ? u.FullName : null,
                        a.SOAPPlanningId,

                        a.IcdId,
                        IcdName = icd != null ? icd.ICDName : null,

                        a.PlanningIcdId,
                        PlanningIcdName = plan != null ? plan.NamaPlanning : null,

                        a.Keterangan
                    };

                var totalRows = await baseQuery.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

                var listdata = await baseQuery
                    .OrderByDescending(a => a.CreateDateTime)
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .ToListAsync();

                if (listdata.Count == 0)
                    return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });

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
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Terjadi kesalahan tak terduga.",
                    error = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var data = await (
                    from a in _applicationDbContext.SOAPPlannings.AsNoTracking()
                        // LEFT JOIN ke ICD10s (PK GUID) -> untuk ambil ICDName
                    join icd in _applicationDbContext.ICD10s.AsNoTracking()
                        on a.IcdId equals icd.ICDId into gi   // <<< GANTI ICD10Id jika nama kolom berbeda
                    from icd in gi.DefaultIfEmpty()

                        // LEFT JOIN ke ICDPlannings -> untuk ambil NamaPlanning
                    join plan in _applicationDbContext.ICDPlannings.AsNoTracking()
                        on a.PlanningIcdId equals plan.ICDPlanningId into gp
                    from plan in gp.DefaultIfEmpty()

                    where a.SOAPPlanningId == id && a.IsDelete != true
                    select new
                    {
                        a.SOAPPlanningId,
                        a.CreateDateTime,
                        a.CreateBy,
                        a.IcdId,
                        IcdName = icd != null ? icd.ICDName : null,
                        a.PlanningIcdId,
                        PlanningIcdName = plan != null ? plan.NamaPlanning : null,
                        a.Keterangan,
                        a.UpdateBy,
                        a.UpdateDateTime,
                        a.DeleteBy,
                        a.DeleteDateTime,
                        a.IsDelete
                    })
                    .FirstOrDefaultAsync();

                if (data == null)
                    return NotFound(new { message = "Data tidak ditemukan. || 404 Not Found" });

                return Ok(new
                {
                    message = "Ditemukan || 200 OK",
                    data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Terjadi kesalahan tak terduga.",
                    error = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SOAPPlanningViewModel vm)
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
                var data = new SOAPPlanning
                {
                    SOAPPlanningId = Guid.NewGuid(),
                    IcdId = vm.IcdId,
                    PlanningIcdId = vm.PlanningIcdId,
                    Keterangan = vm.Keterangan,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                // **Simpan ke Database**
                _applicationDbContext.SOAPPlannings.Add(data);
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
        public async Task<IActionResult> Update(Guid id, [FromBody] SOAPPlanningViewModel vm)
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
                var data = await _applicationDbContext.SOAPPlannings.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Update Data**
                data.IcdId = vm.IcdId;
                data.PlanningIcdId = vm.PlanningIcdId;
                data.Keterangan = vm.Keterangan;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.SOAPPlannings.Update(data);
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
                var data = await _applicationDbContext.SOAPPlannings.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.SOAPPlannings.Update(data);
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

            // Base query + LEFT JOINs (User, ICD10s, ICDPlannings)
            var query =
                from a in _applicationDbContext.SOAPPlannings.AsNoTracking()
                join u in _applicationDbContext.UserActives.AsNoTracking()
                    on a.CreateBy equals u.UserActiveId into gu
                from u in gu.DefaultIfEmpty()
                join icd in _applicationDbContext.ICD10s.AsNoTracking()
                    on a.IcdId equals icd.ICDId into gi   // <<< GANTI ICD10Id jika berbeda
                from icd in gi.DefaultIfEmpty()
                join plan in _applicationDbContext.ICDPlannings.AsNoTracking()
                    on a.PlanningIcdId equals plan.ICDPlanningId into gp
                from plan in gp.DefaultIfEmpty()
                where a.IsDelete != true
                select new
                {
                    a.CreateDateTime,
                    a.CreateBy,
                    CreateByName = u != null ? u.FullName : null,
                    a.SOAPPlanningId,
                    a.IcdId,
                    IcdName = icd != null ? icd.ICDName : null,
                    a.PlanningIcdId,
                    PlanningIcdName = plan != null ? plan.NamaPlanning : null,
                    a.Keterangan
                };

            // Search (ILike) — mendukung 1 huruf
            if (!string.IsNullOrWhiteSpace(search))
            {
                var pattern = $"%{search.Trim()}%";
                query = query.Where(x =>
                    EF.Functions.ILike(x.IcdName ?? "", pattern) ||
                    EF.Functions.ILike(x.PlanningIcdName ?? "", pattern) ||
                    EF.Functions.ILike(x.Keterangan ?? "", pattern) ||
                    EF.Functions.ILike(x.CreateByName ?? "", pattern));
            }

            // Filter rentang tanggal eksplisit
            if (startDate.HasValue || endDate.HasValue)
            {
                // [start, end) — end exclusive agar aman
                var startUtc = (startDate?.Date ?? DateTime.UtcNow.Date);
                var endUtc = (endDate?.Date.AddDays(1) ?? DateTime.UtcNow.Date.AddDays(1));
                query = query.Where(x => x.CreateDateTime >= startUtc && x.CreateDateTime < endUtc);
            }

            // Filter berdasarkan periode (gunakan boundaries, hindari .Date di server)
            if (periode.HasValue)
            {
                var today = DateTime.UtcNow.Date;
                DateTime start, end;

                switch (periode.Value)
                {
                    case PeriodeFilter.Today:
                        start = today;
                        end = today.AddDays(1);
                        break;

                    case PeriodeFilter.ThisWeek:
                        // minggu dimulai Minggu (DayOfWeek.Sunday = 0)
                        start = today.AddDays(-(int)today.DayOfWeek);
                        end = today.AddDays(1); // s.d. hari ini
                        break;

                    case PeriodeFilter.LastWeek:
                        var startThisWeek = today.AddDays(-(int)today.DayOfWeek);
                        start = startThisWeek.AddDays(-7);
                        end = startThisWeek;
                        break;

                    case PeriodeFilter.ThisMonth:
                        start = new DateTime(today.Year, today.Month, 1);
                        end = start.AddMonths(1);
                        break;

                    case PeriodeFilter.LastMonth:
                        start = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
                        end = start.AddMonths(1);
                        break;

                    case PeriodeFilter.ThisYear:
                        start = new DateTime(today.Year, 1, 1);
                        end = start.AddYears(1);
                        break;

                    case PeriodeFilter.LastYear:
                        start = new DateTime(today.Year - 1, 1, 1);
                        end = start.AddYears(1);
                        break;

                    case PeriodeFilter.Last3Months:
                        start = today.AddMonths(-3);
                        end = today.AddDays(1);
                        break;

                    case PeriodeFilter.Last6Months:
                        start = today.AddMonths(-6);
                        end = today.AddDays(1);
                        break;

                    default:
                        start = today;
                        end = today.AddDays(1);
                        break;
                }

                query = query.Where(x => x.CreateDateTime >= start && x.CreateDateTime < end);
            }

            // Sorting (tambahkan kolom yang bisa di-sort)
            bool desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            query = (orderBy?.Trim()) switch
            {
                "CreateByName" => (desc ? query.OrderByDescending(x => x.CreateByName) : query.OrderBy(x => x.CreateByName)),
                "IcdName" => (desc ? query.OrderByDescending(x => x.IcdName) : query.OrderBy(x => x.IcdName)),
                "PlanningIcdName" => (desc ? query.OrderByDescending(x => x.PlanningIcdName) : query.OrderBy(x => x.PlanningIcdName)),
                _ /*CreateDateTime*/ => (desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime)),
            };

            // Pagination
            var totalRows = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var rows = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            if (rows.Count == 0 && page > Math.Max(totalPages, 1))
                return NotFound(new { message = "Page not found." });

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
