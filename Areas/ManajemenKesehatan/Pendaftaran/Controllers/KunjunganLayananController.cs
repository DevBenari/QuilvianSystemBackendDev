using Microsoft.AspNet.SignalR.Client.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using SkiaSharp;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using static Dapper.SqlMapper;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class KunjunganLayananController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<KunjunganLayananController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public KunjunganLayananController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<KunjunganLayananController> logger,
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
            var data = await _applicationDbContext.KunjunganLayanans
                .AsNoTracking()
                .Where(a =>
                    a.KunjunganLayananId == id &&
                    (a.IsDelete == false || a.IsDelete == null))
                .Select(a => new
                {
                    a.CreateDateTime,
                    a.CreateBy,

                    a.KunjunganLayananId,
                    a.KunjunganId,

                    NoRekamMedis = a.Kunjungan != null ? a.Kunjungan.NoRekamMedis : null,
                    JenisKunjungan = a.Kunjungan != null ? a.Kunjungan.JenisKunjungan : null,
                    TipePasien = a.Kunjungan != null ? a.Kunjungan.TipePasien : null,
                    Antrian = a.Kunjungan != null ? a.Kunjungan.Antrian : null,
                    TglMasuk = a.Kunjungan.CreateDateTime ,
                    IsFinished = a.Kunjungan != null ? a.Kunjungan.IsFinished : null,
                    IsClosed = a.Kunjungan != null ? a.Kunjungan.IsClosed : null,

                    PasienId = a.Kunjungan != null ? a.Kunjungan.PasienId : null,

                    a.InstalasiUnitId,
                    KodeInstalasiUnit = a.InstalasiUnit != null
                        ? a.InstalasiUnit.KodeInstalasiUnit
                        : null,
                    NamaInstalasiUnit = a.InstalasiUnit != null
                        ? a.InstalasiUnit.NamaInstalasiUnit
                        : null,

                    a.PoliklinikId,
                    NamaPoliklinik = a.Poliklinik != null
                        ? a.Poliklinik.NamaPoliklinik
                        : null,

                    a.DokterId,
                    KdDokter = a.Dokter != null ? a.Dokter.KdDokter : null,
                    NamaDokter = a.Dokter != null ? a.Dokter.NmDokter : null,
                    Sip = a.Dokter != null ? a.Dokter.Sip : null,
                    Str = a.Dokter != null ? a.Dokter.Str : null,
                    Spesialis = a.Dokter != null ? a.Dokter.Spesialis : null,

                    a.JenisLayanan,
                    a.TglMasukLayanan,
                    a.TglKeluarLayanan,
                    a.IsActive
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(new
                {
                    status = "error",
                    message = "Kunjungan layanan tidak ditemukan."
                });
            }

            return Ok(new
            {
                status = "success",
                message = "Data retrieved successfully",
                data
            });
        }


        [HttpGet("ByKunjungan/{kunjunganId}")]
        public async Task<IActionResult> GetByKunjunganId(Guid kunjunganId)
        {
            var data = await _applicationDbContext.KunjunganLayanans
                .AsNoTracking()
                .Where(a =>
                    a.KunjunganId == kunjunganId &&
                    (a.IsDelete == false || a.IsDelete == null))
                .Select(a => new
                {
                    a.CreateDateTime,
                    a.CreateBy,

                    a.KunjunganLayananId,
                    a.KunjunganId,

                    NoRekamMedis = a.Kunjungan != null ? a.Kunjungan.NoRekamMedis : null,
                    JenisKunjungan = a.Kunjungan != null ? a.Kunjungan.JenisKunjungan : null,
                    TipePasien = a.Kunjungan != null ? a.Kunjungan.TipePasien : null,
                    Antrian = a.Kunjungan != null ? a.Kunjungan.Antrian : null,
                    TglMasuk = a.Kunjungan.CreateDateTime,
                    IsFinished = a.Kunjungan != null ? a.Kunjungan.IsFinished : null,
                    IsClosed = a.Kunjungan != null ? a.Kunjungan.IsClosed : null,

                    PasienId = a.Kunjungan != null ? a.Kunjungan.PasienId : null,

                    a.InstalasiUnitId,
                    KodeInstalasiUnit = a.InstalasiUnit != null
                        ? a.InstalasiUnit.KodeInstalasiUnit
                        : null,
                    NamaInstalasiUnit = a.InstalasiUnit != null
                        ? a.InstalasiUnit.NamaInstalasiUnit
                        : null,

                    a.PoliklinikId,
                    NamaPoliklinik = a.Poliklinik != null
                        ? a.Poliklinik.NamaPoliklinik
                        : null,

                    a.DokterId,
                    KdDokter = a.Dokter != null ? a.Dokter.KdDokter : null,
                    NamaDokter = a.Dokter != null ? a.Dokter.NmDokter : null,
                    Sip = a.Dokter != null ? a.Dokter.Sip : null,
                    Str = a.Dokter != null ? a.Dokter.Str : null,
                    Spesialis = a.Dokter != null ? a.Dokter.Spesialis : null,

                    a.JenisLayanan,
                    a.TglMasukLayanan,
                    a.TglKeluarLayanan,
                    a.IsActive
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(new
                {
                    status = "error",
                    message = "Kunjungan layanan tidak ditemukan."
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
        public async Task<IActionResult> Create([FromBody] KunjunganLayananViewModel vm)
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
                bool isDuplicate = await _applicationDbContext.KunjunganLayanans
                    .AnyAsync(c => c.KunjunganId == vm.KunjunganId &&
                    c.InstalasiUnitId == vm.InstalasiUnitId &&
                    c.IsActive == true &&
                    c.IsDelete == false);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Kunjungan ini masih aktif dalam instalasi unit ini" });
                }

                // **Buat Data Baru**
                var data = new KunjunganLayanan
                {
                    KunjunganLayananId = Guid.NewGuid(),
                    KunjunganId = vm.KunjunganId,
                    InstalasiUnitId = vm.InstalasiUnitId,
                    PoliklinikId = vm.PoliklinikId,
                    DokterId = vm.DokterId,
                    JenisLayanan = vm.JenisLayanan,
                    TglMasukLayanan = vm.TglMasukLayanan,
                    TglKeluarLayanan = vm.TglKeluarLayanan,
                    IsActive = true,

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                // **Simpan ke Database**
                _applicationDbContext.KunjunganLayanans.Add(data);
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
        public async Task<IActionResult> Update(Guid id, [FromBody] KunjunganLayananViewModel vm)
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

                var data = await _applicationDbContext.KunjunganLayanans.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Cari Data**
                bool isDuplicate = await _applicationDbContext.KunjunganLayanans
                    .AnyAsync(c => c.KunjunganId == vm.KunjunganId &&
                    c.InstalasiUnitId == vm.InstalasiUnitId &&
                    c.KunjunganLayananId != id &&
                    c.IsActive == true &&
                    c.IsDelete == false);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Kunjungan ini masih aktif dalam instalasi unit ini" });
                }

                // **Update Data**

                data.InstalasiUnitId = vm.InstalasiUnitId;
                data.PoliklinikId = vm.PoliklinikId;
                data.DokterId = vm.DokterId;
                data.JenisLayanan = vm.JenisLayanan;

                if (vm.TglMasukLayanan.HasValue)
                    // Remove the incorrect usage of HasValue since TglMasukLayanan is of type DateTime (non-nullable)
                    data.TglMasukLayanan = vm.TglMasukLayanan;

                data.TglKeluarLayanan = vm.TglKeluarLayanan;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.KunjunganLayanans.Update(data);
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
            Guid? kunjunganId = null,
            Guid? instalasiUnitId = null,
            Guid? poliklinikId = null,
            Guid? dokterId = null,
            string? jenisLayanan = null,
            bool? isActive = null)
        {
            if (page <= 0) page = 1;
            if (perPage <= 0) perPage = 10;

            var query = _applicationDbContext.KunjunganLayanans
                .AsNoTracking()
                .Where(a => a.IsDelete == false || a.IsDelete == null)
                .Select(a => new
                {
                    a.CreateDateTime,
                    a.CreateBy,

                    a.KunjunganLayananId,
                    a.KunjunganId,

                    NoRekamMedis = a.Kunjungan != null ? a.Kunjungan.NoRekamMedis : null,
                    JenisKunjungan = a.Kunjungan != null ? a.Kunjungan.JenisKunjungan : null,
                    TipePasien = a.Kunjungan != null ? a.Kunjungan.TipePasien : null,

                    a.InstalasiUnitId,
                    KodeInstalasiUnit = a.InstalasiUnit != null ? a.InstalasiUnit.KodeInstalasiUnit : null,
                    NamaInstalasiUnit = a.InstalasiUnit != null ? a.InstalasiUnit.NamaInstalasiUnit : null,

                    a.PoliklinikId,
                    NamaPoliklinik = a.Poliklinik != null ? a.Poliklinik.NamaPoliklinik : null,


                    a.DokterId,
                    KdDokter = a.Dokter != null ? a.Dokter.KdDokter : null,
                    NamaDokter = a.Dokter != null ? a.Dokter.NmDokter : null,

                    a.JenisLayanan,
                    a.TglMasukLayanan,
                    a.TglKeluarLayanan,
                    a.IsActive
                });

            if (kunjunganId.HasValue)
            {
                query = query.Where(a => a.KunjunganId == kunjunganId.Value);
            }

            if (instalasiUnitId.HasValue)
            {
                query = query.Where(a => a.InstalasiUnitId == instalasiUnitId.Value);
            }

            if (poliklinikId.HasValue)
            {
                query = query.Where(a => a.PoliklinikId == poliklinikId.Value);
            }

            if (dokterId.HasValue)
            {
                query = query.Where(a => a.DokterId == dokterId.Value);
            }

            if (!string.IsNullOrWhiteSpace(jenisLayanan))
            {
                var jenis = $"%{jenisLayanan}%";

                query = query.Where(a =>
                    a.JenisLayanan != null &&
                    EF.Functions.ILike(a.JenisLayanan, jenis));
            }

            if (isActive.HasValue)
            {
                query = query.Where(a => a.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchPattern = $"%{search}%";

                query = query.Where(a =>
                    (a.NoRekamMedis != null && EF.Functions.ILike(a.NoRekamMedis, searchPattern)) ||
                    (a.JenisKunjungan != null && EF.Functions.ILike(a.JenisKunjungan, searchPattern)) ||
                    (a.TipePasien != null && EF.Functions.ILike(a.TipePasien, searchPattern)) ||
                    (a.KodeInstalasiUnit != null && EF.Functions.ILike(a.KodeInstalasiUnit, searchPattern)) ||
                    (a.NamaInstalasiUnit != null && EF.Functions.ILike(a.NamaInstalasiUnit, searchPattern)) ||
                    (a.NamaPoliklinik != null && EF.Functions.ILike(a.NamaPoliklinik, searchPattern)) ||
                    (a.KdDokter != null && EF.Functions.ILike(a.KdDokter, searchPattern)) ||
                    (a.NamaDokter != null && EF.Functions.ILike(a.NamaDokter, searchPattern)) ||
                    (a.JenisLayanan != null && EF.Functions.ILike(a.JenisLayanan, searchPattern))
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
                    "JenisLayanan" => query.OrderByDescending(a => a.JenisLayanan),
                    "TglMasukLayanan" => query.OrderByDescending(a => a.TglMasukLayanan),
                    "NamaInstalasiUnit" => query.OrderByDescending(a => a.NamaInstalasiUnit),
                    "NamaPoliklinik" => query.OrderByDescending(a => a.NamaPoliklinik),
                    "NamaDokter" => query.OrderByDescending(a => a.NamaDokter),
                    "NoRekamMedis" => query.OrderByDescending(a => a.NoRekamMedis),
                    _ => query.OrderByDescending(a => a.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderBy(a => a.CreateDateTime),
                    "JenisLayanan" => query.OrderBy(a => a.JenisLayanan),
                    "TglMasukLayanan" => query.OrderBy(a => a.TglMasukLayanan),
                    "NamaInstalasiUnit" => query.OrderBy(a => a.NamaInstalasiUnit),
                    "NamaPoliklinik" => query.OrderBy(a => a.NamaPoliklinik),
                    "NamaDokter" => query.OrderBy(a => a.NamaDokter),
                    "NoRekamMedis" => query.OrderBy(a => a.NoRekamMedis),
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
