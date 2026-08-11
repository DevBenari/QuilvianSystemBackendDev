using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class LabHasilSetBakteriController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<LabHasilSetBakteriController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public LabHasilSetBakteriController(
        ApplicationDbContext applicationDbContext,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<LabHasilSetBakteriController> logger,
        IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id,CancellationToken ct)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new
                {
                    status = "error",
                    message = "Parameter ID tidak valid."
                });
            }

            var data = await _applicationDbContext.LabHasilSetBakteris
                .AsNoTracking()
                .Where(x =>
                    x.LabHasilSetBakteriId == id &&
                    (x.IsDelete == false || x.IsDelete == null))
                .Select(x => new
                {
                    // ==========================================
                    // DATA UTAMA
                    // ==========================================
                    x.LabHasilSetBakteriId,

                    x.LabHasilId,
                    x.KunjunganId,
                    x.PasienId,

                    x.AsalSpecimenId,
                    x.JenisSpecimenId,

                    x.Keterangan,


                    // ==========================================
                    // LAB HASIL
                    // ==========================================
                    LabHasil = x.LabHasil == null
                        ? null
                        : new
                        {
                            x.LabHasil.HasilLabId,
                            x.LabHasil.LabBookingId,
                            x.LabHasil.LabId,
                            x.LabHasil.DokterPerujukId,
                            x.LabHasil.DokterKonfirmatorId,
                            x.LabHasil.PenanggungJawabId,
                            x.LabHasil.PenanggungJawabAnalisId,
                            x.LabHasil.TanggalPemeriksaan
                        },


                    // ==========================================
                    // KUNJUNGAN
                    // ==========================================
                    Kunjungan = x.Kunjungan == null
                        ? null
                        : new
                        {
                            x.Kunjungan.KunjunganID,
                            x.Kunjungan.NoRegistrasi
                        },


                    // ==========================================
                    // PASIEN
                    // ==========================================
                    Pasien = x.Pasien == null
                        ? null
                        : new
                        {
                            x.Pasien.PendaftaranPasienBaruId,
                            x.Pasien.NoRekamMedis,
                            x.Pasien.NamaLengkap
                        },


                    // ==========================================
                    // ASAL SPECIMEN
                    // ==========================================
                    AsalSpecimen = x.AsalSpecimen == null
                        ? null
                        : new
                        {
                            x.AsalSpecimen.SpecimenAsalId,
                            x.AsalSpecimen.AsalSpecimen
                        },


                    // ==========================================
                    // JENIS SPECIMEN
                    // ==========================================
                    JenisSpecimen = x.JenisSpecimen == null
                        ? null
                        : new
                        {
                            x.JenisSpecimen.JenisSpecimenId,
                            x.JenisSpecimen.NamaJenisSpecimen
                        },


                    // ==========================================
                    // AUDIT
                    // ==========================================
                    x.CreateDateTime,
                    x.CreateBy,
                    x.UpdateDateTime,
                    x.UpdateBy,

                    CreateByName = _applicationDbContext.UserActives
                        .Where(u => u.UserActiveId == x.CreateBy)
                        .Select(u => u.FullName)
                        .FirstOrDefault(),

                    UpdateByName = _applicationDbContext.UserActives
                        .Where(u => u.UserActiveId == x.UpdateBy)
                        .Select(u => u.FullName)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync(ct);


            if (data == null)
            {
                return NotFound(new
                {
                    status = "error",
                    message = "Data Lab Hasil Set Bakteri tidak ditemukan."
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
        public async Task<IActionResult> Create([FromBody] LabHasilSetBakteriViewModel vm)
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

                // **Buat Data Baru**
                var data = new LabHasilSetBakteri
                {
                    LabHasilSetBakteriId = Guid.NewGuid(),
                    LabHasilId = vm.LabHasilId,
                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,
                    AsalSpecimenId = vm.AsalSpecimenId,
                    JenisSpecimenId = vm.JenisSpecimenId,
                    Keterangan = vm.Keterangan,

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                // **Simpan ke Database**
                _applicationDbContext.LabHasilSetBakteris.Add(data);
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
        public async Task<IActionResult> Update(Guid id, [FromBody] LabHasilSetBakteriViewModel vm)
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
                var data = await _applicationDbContext.LabHasilSetBakteris.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Update Data**
                data.LabHasilId = vm.LabHasilId;
                data.KunjunganId = vm.KunjunganId;
                data.PasienId = vm.PasienId;
                data.AsalSpecimenId = vm.AsalSpecimenId;
                data.JenisSpecimenId = vm.JenisSpecimenId;
                data.Keterangan = vm.Keterangan;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.LabHasilSetBakteris.Update(data);
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
                var data = await _applicationDbContext.LabHasilSetBakteris.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.LabHasilSetBakteris.Update(data);
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
            [FromQuery, SwaggerSchema(Format = "date-time",Description = "Format: YYYY-MM-DD")]
            DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time",Description = "Format: YYYY-MM-DD")]
            DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))]
            PeriodeFilter? periode = null,
            CancellationToken ct = default)
        {
            // ============================================================
            // Validasi Pagination
            // ============================================================
            if (page < 1)
                page = 1;

            if (perPage < 1)
                perPage = 10;

            if (perPage > 100)
                perPage = 100;


            // ============================================================
            // Query
            // Navigation property akan otomatis diterjemahkan EF menjadi JOIN
            // ============================================================
            var query = _applicationDbContext.LabHasilSetBakteris
                .AsNoTracking()
                .Where(x => x.IsDelete == false || x.IsDelete == null)
                .Select(x => new
                {
                    // ====================================================
                    // PRIMARY DATA
                    // ====================================================
                    x.LabHasilSetBakteriId,

                    x.LabHasilId,
                    x.KunjunganId,
                    x.PasienId,

                    x.AsalSpecimenId,
                    x.JenisSpecimenId,

                    x.Keterangan,


                    // ====================================================
                    // LAB HASIL
                    // ====================================================
                    HasilLabId = x.LabHasil != null
                        ? x.LabHasil.HasilLabId
                        : (Guid?)null,

                    LabBookingId = x.LabHasil != null
                        ? x.LabHasil.LabBookingId
                        : (Guid?)null,

                    TanggalPemeriksaan = x.LabHasil != null
                        ? x.LabHasil.TanggalPemeriksaan
                        : null,


                    // ====================================================
                    // KUNJUNGAN
                    // Sesuaikan NoRegistrasi jika nama property berbeda
                    // ====================================================
                    NoRegistrasi = x.Kunjungan != null
                        ? x.Kunjungan.NoRegistrasi
                        : null,


                    // ====================================================
                    // PASIEN
                    // ====================================================
                    NoRekamMedis = x.Pasien != null
                        ? x.Pasien.NoRekamMedis
                        : null,

                    NamaPasien = x.Pasien != null
                        ? x.Pasien.NamaLengkap
                        : null,


                    // ====================================================
                    // ASAL SPECIMEN
                    // ====================================================
                    NamaAsalSpecimen = x.AsalSpecimen != null
                        ? x.AsalSpecimen.AsalSpecimen
                        : null,


                    // ====================================================
                    // JENIS SPECIMEN
                    // ====================================================
                    NamaJenisSpecimen = x.JenisSpecimen != null
                        ? x.JenisSpecimen.NamaJenisSpecimen
                        : null,


                    // ====================================================
                    // AUDIT
                    // ====================================================
                    x.CreateBy,

                    CreateByName = _applicationDbContext.UserActives
                        .Where(u => u.UserActiveId == x.CreateBy)
                        .Select(u => u.FullName)
                        .FirstOrDefault(),

                    x.CreateDateTime
                });


            // ============================================================
            // SEARCH
            // ============================================================
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = $"%{search.Trim()}%";

                query = query.Where(x =>
                    (x.NamaPasien != null &&
                     EF.Functions.ILike(x.NamaPasien, keyword))

                    ||

                    (x.NoRekamMedis != null &&
                     EF.Functions.ILike(x.NoRekamMedis, keyword))

                    ||

                    (x.NoRegistrasi != null &&
                     EF.Functions.ILike(x.NoRegistrasi, keyword))

                    ||

                    (x.NamaAsalSpecimen != null &&
                     EF.Functions.ILike(x.NamaAsalSpecimen, keyword))

                    ||

                    (x.NamaJenisSpecimen != null &&
                     EF.Functions.ILike(x.NamaJenisSpecimen, keyword))

                    ||

                    (x.Keterangan != null &&
                     EF.Functions.ILike(x.Keterangan, keyword))

                    ||

                    (x.CreateByName != null &&
                     EF.Functions.ILike(x.CreateByName, keyword))
                );
            }


            // ============================================================
            // FILTER RANGE TANGGAL
            // ============================================================
            if (startDate.HasValue && endDate.HasValue)
            {
                var startUtc = startDate.Value.Date.ToUniversalTime();

                // Lebih aman menggunakan < tanggal berikutnya
                // daripada AddTicks(-1)
                var endUtc = endDate.Value.Date
                    .AddDays(1)
                    .ToUniversalTime();

                query = query.Where(x =>
                    x.CreateDateTime >= startUtc &&
                    x.CreateDateTime < endUtc
                );
            }


            // ============================================================
            // FILTER PERIODE
            // ============================================================
            if (periode.HasValue)
            {
                var today = DateTime.UtcNow.Date;

                DateTime? periodeStart = null;
                DateTime? periodeEnd = null;

                switch (periode.Value)
                {
                    case PeriodeFilter.Today:
                        periodeStart = today;
                        periodeEnd = today.AddDays(1);
                        break;


                    case PeriodeFilter.ThisWeek:
                        {
                            var diff = ((int)today.DayOfWeek + 6) % 7;

                            periodeStart = today.AddDays(-diff);
                            periodeEnd = today.AddDays(1);

                            break;
                        }


                    case PeriodeFilter.LastWeek:
                        {
                            var diff = ((int)today.DayOfWeek + 6) % 7;

                            var thisWeekStart = today.AddDays(-diff);

                            periodeStart = thisWeekStart.AddDays(-7);
                            periodeEnd = thisWeekStart;

                            break;
                        }


                    case PeriodeFilter.ThisMonth:
                        periodeStart = new DateTime(
                            today.Year,
                            today.Month,
                            1);

                        periodeEnd = periodeStart.Value.AddMonths(1);
                        break;


                    case PeriodeFilter.LastMonth:
                        {
                            var thisMonthStart = new DateTime(
                                today.Year,
                                today.Month,
                                1);

                            periodeStart = thisMonthStart.AddMonths(-1);
                            periodeEnd = thisMonthStart;

                            break;
                        }


                    case PeriodeFilter.ThisYear:
                        periodeStart = new DateTime(
                            today.Year,
                            1,
                            1);

                        periodeEnd = periodeStart.Value.AddYears(1);
                        break;


                    case PeriodeFilter.LastYear:
                        periodeStart = new DateTime(
                            today.Year - 1,
                            1,
                            1);

                        periodeEnd = new DateTime(
                            today.Year,
                            1,
                            1);

                        break;


                    case PeriodeFilter.Last3Months:
                        periodeStart = today.AddMonths(-3);
                        periodeEnd = today.AddDays(1);
                        break;


                    case PeriodeFilter.Last6Months:
                        periodeStart = today.AddMonths(-6);
                        periodeEnd = today.AddDays(1);
                        break;
                }


                if (periodeStart.HasValue)
                {
                    query = query.Where(x =>
                        x.CreateDateTime >= periodeStart.Value);
                }

                if (periodeEnd.HasValue)
                {
                    query = query.Where(x =>
                        x.CreateDateTime < periodeEnd.Value);
                }
            }


            // ============================================================
            // SORTING
            // ============================================================
            var descending = string.Equals(
                sortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase);


            query = (orderBy?.ToLower(), descending) switch
            {
                ("namapasien", true)
                    => query.OrderByDescending(x => x.NamaPasien),

                ("namapasien", false)
                    => query.OrderBy(x => x.NamaPasien),


                ("norekammedis", true)
                    => query.OrderByDescending(x => x.NoRekamMedis),

                ("norekammedis", false)
                    => query.OrderBy(x => x.NoRekamMedis),


                ("noregistrasi", true)
                    => query.OrderByDescending(x => x.NoRegistrasi),

                ("noregistrasi", false)
                    => query.OrderBy(x => x.NoRegistrasi),


                ("namaasalspecimen", true)
                    => query.OrderByDescending(x => x.NamaAsalSpecimen),

                ("namaasalspecimen", false)
                    => query.OrderBy(x => x.NamaAsalSpecimen),


                ("namajenisspecimen", true)
                    => query.OrderByDescending(x => x.NamaJenisSpecimen),

                ("namajenisspecimen", false)
                    => query.OrderBy(x => x.NamaJenisSpecimen),


                ("createbyname", true)
                    => query.OrderByDescending(x => x.CreateByName),

                ("createbyname", false)
                    => query.OrderBy(x => x.CreateByName),


                ("createDatetime", true)
                    => query.OrderByDescending(x => x.CreateDateTime),

                ("createDatetime", false)
                    => query.OrderBy(x => x.CreateDateTime),


                _
                    => query.OrderByDescending(x => x.CreateDateTime)
            };


            // ============================================================
            // TOTAL DATA
            // ============================================================
            var totalRows = await query.CountAsync(ct);

            var totalPages = totalRows == 0
                ? 0
                : (int)Math.Ceiling(totalRows / (double)perPage);


            // ============================================================
            // PAGE TIDAK DITEMUKAN
            // ============================================================
            if (totalRows > 0 && page > totalPages)
            {
                return NotFound(new
                {
                    status = "error",
                    message = "Page not found."
                });
            }


            // ============================================================
            // PAGINATION
            // ============================================================
            var rows = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync(ct);


            // ============================================================
            // RESPONSE
            // ============================================================
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
