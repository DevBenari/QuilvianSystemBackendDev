using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.HRD.MasterData.Models;
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
    public class ObatUnitController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<ObatUnitController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ObatUnitController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ObatUnitController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _applicationDbContext.ObatUnits
                .AsNoTracking()
                .Where(a =>
                    a.ObatUnitId == id &&
                    (a.IsDelete == false || a.IsDelete == null))
                .Select(a => new
                {
                    a.CreateDateTime,
                    a.CreateBy,

                    a.ObatUnitId,

                    a.ObatId,
                    ObatCode = a.Obat != null ? a.Obat.ObatCode : null,
                    ObatName = a.Obat != null ? a.Obat.ObatName : null,

                    a.InstalasiUnitId,
                    KodeInstalasiUnit = a.InstalasiUnit != null
                        ? a.InstalasiUnit.KodeInstalasiUnit
                        : null,
                    NamaInstalasiUnit = a.InstalasiUnit != null
                        ? a.InstalasiUnit.NamaInstalasiUnit
                        : null,

                    a.Qty,
                    a.QtyAmbil,
                    a.QtyTersedia
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(new
                {
                    status = "error",
                    message = "Obat unit tidak ditemukan."
                });
            }

            return Ok(new
            {
                status = "success",
                message = "Data retrieved successfully",
                data
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ObatUnitViewModel vm)
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

                //cek duplikasi
                bool isDuplicate = await _applicationDbContext.ObatUnits
                    .AnyAsync(c => c.ObatId == vm.ObatId
                    && c.InstalasiUnitId == vm.InstalasiUnitId
                    && c.IsDelete == false);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Obat sudah tersedia pada unit ini" });
                }

                // **Buat Data Baru**
                var data = new ObatUnit
                {
                    ObatUnitId = Guid.NewGuid(),
                    ObatId = vm.ObatId,
                    InstalasiUnitId = vm.InstalasiUnitId,
                    Qty = vm.Qty,
                    QtyAmbil = vm.QtyAmbil,
                    QtyTersedia = vm.QtyTersedia,

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                // **Simpan ke Database**
                _applicationDbContext.ObatUnits.Add(data);
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
        public async Task<IActionResult> Update(Guid id, [FromBody] ObatUnitViewModel vm)
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
                var data = await _applicationDbContext.ObatUnits.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                //cek duplikasi
                bool isDuplicate = await _applicationDbContext.ObatUnits
                    .AnyAsync(c => c.ObatId == vm.ObatId
                    && c.InstalasiUnitId == vm.InstalasiUnitId
                    && c.ObatUnitId != id
                    && c.IsDelete == false);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Obat sudah tersedia pada unit ini" });
                }

                // **Update Data**
                data.ObatId = vm.ObatId;
                data.InstalasiUnitId = vm.InstalasiUnitId;
                data.Qty = vm.Qty;
                data.QtyAmbil = vm.QtyAmbil;
                data.QtyTersedia = vm.QtyTersedia;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.ObatUnits.Update(data);
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
                var data = await _applicationDbContext.ObatUnits.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.ObatUnits.Update(data);
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
            Guid? obatId = null,
            Guid? instalasiUnitId = null)
        {
            if (page <= 0) page = 1;
            if (perPage <= 0) perPage = 10;

            var query = _applicationDbContext.ObatUnits
                .AsNoTracking()
                .Where(a => a.IsDelete == false || a.IsDelete == null)
                .Select(a => new
                {
                    a.CreateDateTime,
                    a.CreateBy,

                    a.ObatUnitId,

                    a.ObatId,
                    ObatCode = a.Obat != null ? a.Obat.ObatCode : null,
                    ObatName = a.Obat != null ? a.Obat.ObatName : null,

                    a.InstalasiUnitId,
                    KodeInstalasiUnit = a.InstalasiUnit != null
                        ? a.InstalasiUnit.KodeInstalasiUnit
                        : null,
                    NamaInstalasiUnit = a.InstalasiUnit != null
                        ? a.InstalasiUnit.NamaInstalasiUnit
                        : null,

                    a.Qty,
                    a.QtyAmbil,
                    a.QtyTersedia,

                    QtyAvailable = a.QtyTersedia,
                    QtyPick = a.QtyAmbil
                });

            if (obatId.HasValue)
            {
                query = query.Where(a => a.ObatId == obatId.Value);
            }

            if (instalasiUnitId.HasValue)
            {
                query = query.Where(a => a.InstalasiUnitId == instalasiUnitId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchPattern = $"%{search}%";

                query = query.Where(a =>
                    (a.ObatCode != null && EF.Functions.ILike(a.ObatCode, searchPattern)) ||
                    (a.ObatName != null && EF.Functions.ILike(a.ObatName, searchPattern)) ||
                    (a.KodeInstalasiUnit != null && EF.Functions.ILike(a.KodeInstalasiUnit, searchPattern)) ||
                    (a.NamaInstalasiUnit != null && EF.Functions.ILike(a.NamaInstalasiUnit, searchPattern))
                );
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                query = query.Where(a =>
                    a.CreateDateTime >= startUtc &&
                    a.CreateDateTime <= endUtc);
            }

            if (periode.HasValue)
            {
                DateTime today = DateTime.UtcNow.Date;

                switch (periode)
                {
                    case PeriodeFilter.Today:
                        query = query.Where(a => a.CreateDateTime.Date == today);
                        break;

                    case PeriodeFilter.ThisWeek:
                        query = query.Where(a =>
                            a.CreateDateTime.Date >= today.AddDays(-(int)today.DayOfWeek) &&
                            a.CreateDateTime.Date <= today);
                        break;

                    case PeriodeFilter.LastWeek:
                        query = query.Where(a =>
                            a.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                            a.CreateDateTime.Date < today.AddDays(-(int)today.DayOfWeek));
                        break;

                    case PeriodeFilter.ThisMonth:
                        query = query.Where(a =>
                            a.CreateDateTime.Month == today.Month &&
                            a.CreateDateTime.Year == today.Year);
                        break;

                    case PeriodeFilter.LastMonth:
                        var lastMonth = today.AddMonths(-1);

                        query = query.Where(a =>
                            a.CreateDateTime.Month == lastMonth.Month &&
                            a.CreateDateTime.Year == lastMonth.Year);
                        break;

                    case PeriodeFilter.ThisYear:
                        query = query.Where(a => a.CreateDateTime.Year == today.Year);
                        break;

                    case PeriodeFilter.LastYear:
                        query = query.Where(a => a.CreateDateTime.Year == today.Year - 1);
                        break;

                    case PeriodeFilter.Last3Months:
                        query = query.Where(a => a.CreateDateTime >= today.AddMonths(-3));
                        break;

                    case PeriodeFilter.Last6Months:
                        query = query.Where(a => a.CreateDateTime >= today.AddMonths(-6));
                        break;
                }
            }

            query = sortDirection?.ToLower() == "desc"
                ? orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(a => a.CreateDateTime),
                    "KodeInstalasiUnit" => query.OrderByDescending(a => a.KodeInstalasiUnit),
                    "NamaInstalasiUnit" => query.OrderByDescending(a => a.NamaInstalasiUnit),
                    "Qty" => query.OrderByDescending(a => a.Qty),
                    "QtyAmbil" => query.OrderByDescending(a => a.QtyAmbil),
                    "QtyTersedia" => query.OrderByDescending(a => a.QtyTersedia),
                    _ => query.OrderByDescending(a => a.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderBy(a => a.CreateDateTime),
                    "KodeInstalasiUnit" => query.OrderBy(a => a.KodeInstalasiUnit),
                    "NamaInstalasiUnit" => query.OrderBy(a => a.NamaInstalasiUnit),
                    "Qty" => query.OrderBy(a => a.Qty),
                    "QtyAmbil" => query.OrderBy(a => a.QtyAmbil),
                    "QtyTersedia" => query.OrderBy(a => a.QtyTersedia),
                    _ => query.OrderBy(a => a.CreateDateTime)
                };

            var totalRows = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var rows = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            if (rows.Count == 0 && page > totalPages && totalRows > 0)
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
