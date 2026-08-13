using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels;
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
    public class LabNilaiRujukanController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly ILogger<LabNilaiRujukanController> _logger;

        public LabNilaiRujukanController(
            ApplicationDbContext applicationDbContext,
            ILogger<LabNilaiRujukanController> logger)
        {
            _applicationDbContext = applicationDbContext;
            _logger = logger;
        }

        // ============================================================
        // GET ALL
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> GetAll(
            int page = 1,
            int perPage = 10)
        {
            if (page < 1)
                page = 1;

            if (perPage < 1)
                perPage = 10;

            var query =
                from a in _applicationDbContext.LabNilaiRujukans

                join user in _applicationDbContext.UserActives
                    on a.CreateBy equals user.UserActiveId into userJoin

                from u in userJoin.DefaultIfEmpty()

                where a.IsDelete == false ||
                      a.IsDelete == null

                select new
                {
                    a.LabNilaiRujukanId,

                    // =========================
                    // PEMERIKSAAN LAB
                    // =========================

                    a.PemeriksaanLabId,

                    KodePemeriksaanLab =
                        a.PemeriksaanLab != null
                            ? a.PemeriksaanLab.KodePemeriksaan
                            : null,

                    NamaPemeriksaanLab =
                        a.PemeriksaanLab != null
                            ? a.PemeriksaanLab.NamaPemeriksaan
                            : null,

                    // =========================
                    // NILAI RUJUKAN
                    // =========================

                    a.JenisKelamin,

                    a.DariUmur,

                    a.SampaiUmur,

                    a.NilaiMinimum,

                    a.NilaiMaximum,

                    a.NilaiNormal,

                    a.HasilNilaiNormal,

                    a.StatusNilaiNormal,

                    a.Keterangan,

                    // =========================
                    // AUDIT
                    // =========================

                    a.CreateDateTime,

                    a.CreateBy,

                    CreateByName =
                        u != null
                            ? u.FullName
                            : null,

                    a.UpdateDateTime,

                    a.UpdateBy
                };

            query = query
                .OrderByDescending(x => x.CreateDateTime);

            var totalRows =
                await query.CountAsync();

            var totalPages =
                (int)Math.Ceiling(
                    totalRows / (double)perPage);

            var listdata =
                await query
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .ToListAsync();

            if (!listdata.Any())
            {
                return NotFound(new
                {
                    message =
                        "Belum ada data atau halaman tidak ditemukan. || 404 Not Found"
                });
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

        // ============================================================
        // GET BY ID
        // ============================================================

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var listdata = await (
                from a in _applicationDbContext.LabNilaiRujukans

                join user in _applicationDbContext.UserActives
                    on a.CreateBy equals user.UserActiveId into userJoin

                from u in userJoin.DefaultIfEmpty()

                where
                    a.LabNilaiRujukanId == id
                    &&
                    (a.IsDelete == false ||
                     a.IsDelete == null)

                select new
                {
                    a.LabNilaiRujukanId,

                    // =========================
                    // PEMERIKSAAN
                    // =========================

                    a.PemeriksaanLabId,

                    KodePemeriksaanLab =
                        a.PemeriksaanLab != null
                            ? a.PemeriksaanLab.KodePemeriksaan
                            : null,

                    NamaPemeriksaanLab =
                        a.PemeriksaanLab != null
                            ? a.PemeriksaanLab.NamaPemeriksaan
                            : null,

                    // =========================
                    // PARAMETER RUJUKAN
                    // =========================

                    a.JenisKelamin,

                    a.DariUmur,

                    a.SampaiUmur,

                    a.NilaiMinimum,

                    a.NilaiMaximum,

                    a.NilaiNormal,

                    a.HasilNilaiNormal,

                    a.StatusNilaiNormal,

                    a.Keterangan,

                    // =========================
                    // AUDIT
                    // =========================

                    a.CreateDateTime,

                    a.CreateBy,

                    CreateByName =
                        u != null
                            ? u.FullName
                            : null,

                    a.UpdateDateTime,

                    a.UpdateBy,

                    a.DeleteDateTime,

                    a.DeleteBy,

                    a.IsDelete
                }
            ).FirstOrDefaultAsync();

            if (listdata == null)
            {
                return NotFound(new
                {
                    message = "Data tidak ditemukan."
                });
            }

            return Ok(new
            {
                message = "Ditemukan || 200 OK",
                data = listdata
            });
        }

        // ============================================================
        // CREATE
        // ============================================================

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] LabNilaiRujukanViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Data tidak valid."
                });
            }

            try
            {
                // ==========================================
                // CEK DATABASE
                // ==========================================

                if (!await _applicationDbContext
                    .Database
                    .CanConnectAsync())
                {
                    return StatusCode(500, new
                    {
                        message =
                            "Tidak dapat terhubung ke database."
                    });
                }

                // ==========================================
                // USER LOGIN
                // ==========================================

                var emailLogin = User
                    .FindFirst(ClaimTypes.NameIdentifier)?
                    .Value;

                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new
                    {
                        message =
                            "User tidak terautentikasi!"
                    });
                }

                var getUserActive =
                    await _applicationDbContext
                        .UserActives
                        .FirstOrDefaultAsync(x =>
                            x.Email == emailLogin);

                if (getUserActive == null)
                {
                    return Unauthorized(new
                    {
                        message =
                            "User aktif tidak ditemukan!"
                    });
                }

                var userActiveId =
                    getUserActive.UserActiveId;

                // ==========================================
                // VALIDASI PEMERIKSAAN LAB
                // ==========================================

                if (!vm.PemeriksaanLabId.HasValue ||
                    vm.PemeriksaanLabId == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        message =
                            "PemeriksaanLabId wajib diisi."
                    });
                }

                // Sesuaikan nama DbSet bila berbeda
                var pemeriksaanExists =
                    await _applicationDbContext
                        .LabPemeriksaans
                        .AnyAsync(x =>
                            x.PemeriksaanLabId ==
                            vm.PemeriksaanLabId.Value);

                if (!pemeriksaanExists)
                {
                    return BadRequest(new
                    {
                        message =
                            "PemeriksaanLabId tidak ditemukan."
                    });
                }

                // ==========================================
                // VALIDASI RANGE NILAI
                // ==========================================

                if (vm.NilaiMinimum.HasValue &&
                    vm.NilaiMaximum.HasValue &&
                    vm.NilaiMinimum >
                    vm.NilaiMaximum)
                {
                    return BadRequest(new
                    {
                        message =
                            "NilaiMinimum tidak boleh lebih besar dari NilaiMaximum."
                    });
                }

                // ==========================================
                // VALIDASI UMUR
                // ==========================================

                if (vm.DariUmur.HasValue &&
                    vm.SampaiUmur.HasValue &&
                    vm.DariUmur >
                    vm.SampaiUmur)
                {
                    return BadRequest(new
                    {
                        message =
                            "DariUmur tidak boleh lebih besar dari SampaiUmur."
                    });
                }

                // ==========================================
                // CREATE
                // ==========================================

                var data = new LabNilaiRujukan
                {
                    LabNilaiRujukanId =
                        Guid.NewGuid(),

                    PemeriksaanLabId =
                        vm.PemeriksaanLabId,

                    JenisKelamin =
                        vm.JenisKelamin,

                    DariUmur =
                        vm.DariUmur,

                    SampaiUmur =
                        vm.SampaiUmur,

                    NilaiMinimum =
                        vm.NilaiMinimum,

                    NilaiMaximum =
                        vm.NilaiMaximum,

                    NilaiNormal =
                        vm.NilaiNormal,

                    HasilNilaiNormal =
                        vm.HasilNilaiNormal,

                    StatusNilaiNormal =
                        vm.StatusNilaiNormal,

                    Keterangan =
                        vm.Keterangan,

                    CreateBy =
                        userActiveId,

                    CreateDateTime =
                        DateTimeOffset.UtcNow,

                    IsDelete =
                        false
                };

                await _applicationDbContext
                    .LabNilaiRujukans
                    .AddAsync(data);

                var result =
                    await _applicationDbContext
                        .SaveChangesAsync();

                if (result > 0)
                {
                    return Created("", new
                    {
                        message =
                            "Tambah Data Berhasil || 201 Created",

                        data = new
                        {
                            data.LabNilaiRujukanId
                        }
                    });
                }

                return StatusCode(500, new
                {
                    message =
                        "Data tidak berhasil disimpan ke database."
                });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new
                {
                    message =
                        "Gagal menyimpan data.",

                    detail =
                        dbEx.InnerException?.Message
                        ?? dbEx.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message =
                        "Terjadi kesalahan internal.",

                    detail = ex.Message
                });
            }
        }

        // ============================================================
        // UPDATE
        // ============================================================

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] LabNilaiRujukanViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Data tidak valid."
                });
            }

            try
            {
                if (!await _applicationDbContext
                    .Database
                    .CanConnectAsync())
                {
                    return StatusCode(500, new
                    {
                        message =
                            "Tidak dapat terhubung ke database."
                    });
                }

                // ==========================================
                // USER LOGIN
                // ==========================================

                var emailLogin = User
                    .FindFirst(ClaimTypes.NameIdentifier)?
                    .Value;

                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new
                    {
                        message =
                            "User tidak terautentikasi!"
                    });
                }

                var getUserActive =
                    await _applicationDbContext
                        .UserActives
                        .FirstOrDefaultAsync(x =>
                            x.Email == emailLogin);

                if (getUserActive == null)
                {
                    return Unauthorized(new
                    {
                        message =
                            "User aktif tidak ditemukan!"
                    });
                }

                var userActiveId =
                    getUserActive.UserActiveId;

                // ==========================================
                // CARI DATA
                // ==========================================

                var data =
                    await _applicationDbContext
                        .LabNilaiRujukans
                        .FirstOrDefaultAsync(x =>
                            x.LabNilaiRujukanId == id
                            &&
                            (x.IsDelete == false ||
                             x.IsDelete == null));

                if (data == null)
                {
                    return NotFound(new
                    {
                        message =
                            "Data tidak ditemukan."
                    });
                }

                // ==========================================
                // VALIDASI PEMERIKSAAN
                // ==========================================

                if (!vm.PemeriksaanLabId.HasValue ||
                    vm.PemeriksaanLabId == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        message =
                            "PemeriksaanLabId wajib diisi."
                    });
                }

                var pemeriksaanExists =
                    await _applicationDbContext
                        .LabPemeriksaans
                        .AnyAsync(x =>
                            x.PemeriksaanLabId ==
                            vm.PemeriksaanLabId.Value);

                if (!pemeriksaanExists)
                {
                    return BadRequest(new
                    {
                        message =
                            "PemeriksaanLabId tidak ditemukan."
                    });
                }

                // ==========================================
                // VALIDASI NILAI
                // ==========================================

                if (vm.NilaiMinimum.HasValue &&
                    vm.NilaiMaximum.HasValue &&
                    vm.NilaiMinimum >
                    vm.NilaiMaximum)
                {
                    return BadRequest(new
                    {
                        message =
                            "NilaiMinimum tidak boleh lebih besar dari NilaiMaximum."
                    });
                }

                if (vm.DariUmur.HasValue &&
                    vm.SampaiUmur.HasValue &&
                    vm.DariUmur >
                    vm.SampaiUmur)
                {
                    return BadRequest(new
                    {
                        message =
                            "DariUmur tidak boleh lebih besar dari SampaiUmur."
                    });
                }

                // ==========================================
                // UPDATE
                // ==========================================

                data.PemeriksaanLabId =
                    vm.PemeriksaanLabId;

                data.JenisKelamin =
                    vm.JenisKelamin;

                data.DariUmur =
                    vm.DariUmur;

                data.SampaiUmur =
                    vm.SampaiUmur;

                data.NilaiMinimum =
                    vm.NilaiMinimum;

                data.NilaiMaximum =
                    vm.NilaiMaximum;

                data.NilaiNormal =
                    vm.NilaiNormal;

                data.HasilNilaiNormal =
                    vm.HasilNilaiNormal;

                data.StatusNilaiNormal =
                    vm.StatusNilaiNormal;

                data.Keterangan =
                    vm.Keterangan;

                data.UpdateBy =
                    userActiveId;

                data.UpdateDateTime =
                    DateTimeOffset.UtcNow;

                _applicationDbContext
                    .LabNilaiRujukans
                    .Update(data);

                var result =
                    await _applicationDbContext
                        .SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new
                    {
                        message =
                            "Update Data Berhasil || 200 OK"
                    });
                }

                return StatusCode(500, new
                {
                    message =
                        "Data tidak berhasil diperbarui."
                });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new
                {
                    message =
                        "Gagal memperbarui data.",

                    detail =
                        dbEx.InnerException?.Message
                        ?? dbEx.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message =
                        "Terjadi kesalahan internal.",

                    detail = ex.Message
                });
            }
        }

        // ============================================================
        // DELETE - SOFT DELETE
        // ============================================================

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                if (!await _applicationDbContext
                    .Database
                    .CanConnectAsync())
                {
                    return StatusCode(500, new
                    {
                        message =
                            "Tidak dapat terhubung ke database."
                    });
                }

                var emailLogin = User
                    .FindFirst(ClaimTypes.NameIdentifier)?
                    .Value;

                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new
                    {
                        message =
                            "User tidak terautentikasi!"
                    });
                }

                var getUserActive =
                    await _applicationDbContext
                        .UserActives
                        .FirstOrDefaultAsync(x =>
                            x.Email == emailLogin);

                if (getUserActive == null)
                {
                    return Unauthorized(new
                    {
                        message =
                            "User aktif tidak ditemukan!"
                    });
                }

                var userActiveId =
                    getUserActive.UserActiveId;

                var data =
                    await _applicationDbContext
                        .LabNilaiRujukans
                        .FirstOrDefaultAsync(x =>
                            x.LabNilaiRujukanId == id);

                if (data == null)
                {
                    return NotFound(new
                    {
                        message =
                            "Data tidak ditemukan."
                    });
                }

                if (data.IsDelete == true)
                {
                    return BadRequest(new
                    {
                        message =
                            "Data sudah dihapus sebelumnya."
                    });
                }

                data.DeleteBy =
                    userActiveId;

                data.DeleteDateTime =
                    DateTimeOffset.UtcNow;

                data.IsDelete =
                    true;

                _applicationDbContext
                    .LabNilaiRujukans
                    .Update(data);

                var result =
                    await _applicationDbContext
                        .SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new
                    {
                        message =
                            "Data berhasil dihapus (soft delete) || 200 OK"
                    });
                }

                return StatusCode(500, new
                {
                    message =
                        "Data tidak berhasil diperbarui."
                });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new
                {
                    message =
                        "Gagal menghapus data.",

                    detail =
                        dbEx.InnerException?.Message
                        ?? dbEx.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message =
                        "Terjadi kesalahan internal.",

                    detail = ex.Message
                });
            }
        }

        // ============================================================
        // PAGED
        // ============================================================

        [HttpGet("paged")]
        public async Task<IActionResult> Paged(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? jenisKelamin = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",

            [FromQuery,
             SwaggerSchema(
                 Format = "date-time",
                 Description = "Format: YYYY-MM-DD")]
            DateTime? startDate = null,

            [FromQuery,
             SwaggerSchema(
                 Format = "date-time",
                 Description = "Format: YYYY-MM-DD")]
            DateTime? endDate = null,

            [FromQuery,
             JsonConverter(typeof(StringEnumConverter))]
            PeriodeFilter? periode = null)
        {
            if (page < 1)
                page = 1;

            if (perPage < 1)
                perPage = 10;

            var query =
                from a in _applicationDbContext.LabNilaiRujukans

                join user in _applicationDbContext.UserActives
                    on a.CreateBy equals user.UserActiveId into userJoin

                from u in userJoin.DefaultIfEmpty()

                where a.IsDelete == false ||
                      a.IsDelete == null

                select new
                {
                    a.LabNilaiRujukanId,

                    a.PemeriksaanLabId,

                    KodePemeriksaanLab =
                        a.PemeriksaanLab != null
                            ? a.PemeriksaanLab.KodePemeriksaan
                            : null,

                    NamaPemeriksaanLab =
                        a.PemeriksaanLab != null
                            ? a.PemeriksaanLab.NamaPemeriksaan
                            : null,

                    a.JenisKelamin,

                    a.DariUmur,

                    a.SampaiUmur,

                    a.NilaiMinimum,

                    a.NilaiMaximum,

                    a.NilaiNormal,

                    a.HasilNilaiNormal,

                    a.StatusNilaiNormal,

                    a.Keterangan,

                    a.CreateDateTime,

                    a.CreateBy,

                    CreateByName =
                        u != null
                            ? u.FullName
                            : null
                };

            // ========================================================
            // SEARCH
            // ========================================================

            if (!string.IsNullOrWhiteSpace(search))
            {
                var pattern =
                    $"%{search.Trim()}%";

                query = query.Where(x =>

                    EF.Functions.ILike(
                        x.KodePemeriksaanLab ?? "",
                        pattern)

                    ||

                    EF.Functions.ILike(
                        x.NamaPemeriksaanLab ?? "",
                        pattern)

                    ||

                    EF.Functions.ILike(
                        x.NilaiNormal ?? "",
                        pattern)

                    ||

                    EF.Functions.ILike(
                        x.HasilNilaiNormal ?? "",
                        pattern)

                    ||

                    EF.Functions.ILike(
                        x.StatusNilaiNormal ?? "",
                        pattern)

                    ||

                    EF.Functions.ILike(
                        x.Keterangan ?? "",
                        pattern)
                );
            }

            // ========================================================
            // FILTER JENIS KELAMIN
            // ========================================================

            if (!string.IsNullOrWhiteSpace(jenisKelamin))
            {
                query = query.Where(x =>
                    x.JenisKelamin != null &&
                    x.JenisKelamin.ToLower() ==
                    jenisKelamin.ToLower());
            }

            // ========================================================
            // FILTER TANGGAL CREATE
            // ========================================================

            if (startDate.HasValue &&
                endDate.HasValue)
            {
                var startUtc =
                    new DateTimeOffset(
                        startDate.Value.Date,
                        TimeSpan.Zero);

                var endUtc =
                    new DateTimeOffset(
                        endDate.Value.Date
                            .AddDays(1)
                            .AddTicks(-1),
                        TimeSpan.Zero);

                query = query.Where(x =>
                    x.CreateDateTime >= startUtc &&
                    x.CreateDateTime <= endUtc);
            }

            // ========================================================
            // FILTER PERIODE
            // ========================================================

            if (periode.HasValue)
            {
                var today =
                    DateTime.UtcNow.Date;

                switch (periode)
                {
                    case PeriodeFilter.Today:

                        query = query.Where(x =>
                            x.CreateDateTime.Date ==
                            today);

                        break;

                    case PeriodeFilter.ThisWeek:

                        query = query.Where(x =>
                            x.CreateDateTime.Date >=
                            today.AddDays(
                                -(int)today.DayOfWeek)
                            &&
                            x.CreateDateTime.Date <=
                            today);

                        break;

                    case PeriodeFilter.LastWeek:

                        query = query.Where(x =>
                            x.CreateDateTime.Date >=
                            today.AddDays(
                                -7 -
                                (int)today.DayOfWeek)
                            &&
                            x.CreateDateTime.Date <
                            today.AddDays(
                                -(int)today.DayOfWeek));

                        break;

                    case PeriodeFilter.ThisMonth:

                        query = query.Where(x =>
                            x.CreateDateTime.Month ==
                            today.Month
                            &&
                            x.CreateDateTime.Year ==
                            today.Year);

                        break;

                    case PeriodeFilter.LastMonth:

                        var lastMonth =
                            today.AddMonths(-1);

                        query = query.Where(x =>
                            x.CreateDateTime.Month ==
                            lastMonth.Month
                            &&
                            x.CreateDateTime.Year ==
                            lastMonth.Year);

                        break;

                    case PeriodeFilter.ThisYear:

                        query = query.Where(x =>
                            x.CreateDateTime.Year ==
                            today.Year);

                        break;

                    case PeriodeFilter.LastYear:

                        query = query.Where(x =>
                            x.CreateDateTime.Year ==
                            today.Year - 1);

                        break;

                    case PeriodeFilter.Last3Months:

                        query = query.Where(x =>
                            x.CreateDateTime >=
                            today.AddMonths(-3));

                        break;

                    case PeriodeFilter.Last6Months:

                        query = query.Where(x =>
                            x.CreateDateTime >=
                            today.AddMonths(-6));

                        break;
                }
            }

            // ========================================================
            // SORTING
            // ========================================================

            query =
                sortDirection?.ToLower() == "desc"
                    ? orderBy switch
                    {
                        "CreateDateTime" =>
                            query.OrderByDescending(
                                x => x.CreateDateTime),

                        "NamaPemeriksaanLab" =>
                            query.OrderByDescending(
                                x => x.NamaPemeriksaanLab),

                        "JenisKelamin" =>
                            query.OrderByDescending(
                                x => x.JenisKelamin),

                        "NilaiMinimum" =>
                            query.OrderByDescending(
                                x => x.NilaiMinimum),

                        "NilaiMaximum" =>
                            query.OrderByDescending(
                                x => x.NilaiMaximum),

                        _ =>
                            query.OrderByDescending(
                                x => x.CreateDateTime)
                    }
                    : orderBy switch
                    {
                        "CreateDateTime" =>
                            query.OrderBy(
                                x => x.CreateDateTime),

                        "NamaPemeriksaanLab" =>
                            query.OrderBy(
                                x => x.NamaPemeriksaanLab),

                        "JenisKelamin" =>
                            query.OrderBy(
                                x => x.JenisKelamin),

                        "NilaiMinimum" =>
                            query.OrderBy(
                                x => x.NilaiMinimum),

                        "NilaiMaximum" =>
                            query.OrderBy(
                                x => x.NilaiMaximum),

                        _ =>
                            query.OrderBy(
                                x => x.CreateDateTime)
                    };

            // ========================================================
            // PAGINATION
            // ========================================================

            var totalRows =
                await query.CountAsync();

            var totalPages =
                (int)Math.Ceiling(
                    totalRows / (double)perPage);

            var rows =
                await query
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .ToListAsync();

            if (rows.Count == 0 &&
                page > totalPages)
            {
                return NotFound(new
                {
                    message = "Page not found."
                });
            }

            return Ok(new
            {
                status = "success",

                message =
                    "Data retrieved successfully",

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