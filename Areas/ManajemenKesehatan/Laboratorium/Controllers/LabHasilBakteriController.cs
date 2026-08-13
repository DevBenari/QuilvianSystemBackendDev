using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
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
    public class LabHasilBakteriController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LabHasilBakteriController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public LabHasilBakteriController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<LabHasilBakteriController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
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
                from a in _applicationDbContext.LabHasilBakteris

                join user in _applicationDbContext.UserActives
                    on a.CreateBy equals user.UserActiveId into userJoin

                from u in userJoin.DefaultIfEmpty()

                where a.IsDelete == false ||
                      a.IsDelete == null

                select new
                {
                    a.LabHasilBakteriId,

                    a.LabHasilId,

                    a.KunjunganId,

                    NoRegistrasi =
                        a.Kunjungan != null
                            ? a.Kunjungan.NoRegistrasi
                            : null,

                    JenisKunjungan =
                        a.Kunjungan != null
                            ? a.Kunjungan.JenisKunjungan
                            : null,

                    AsalKunjungan =
                        a.Kunjungan != null
                            ? a.Kunjungan.AsalKunjungan
                            : null,

                    a.PasienId,

                    NamaPasien =
                        a.Pasien != null
                            ? a.Pasien.NamaLengkap
                            : null,

                    NoRM =
                        a.Pasien != null
                            ? a.Pasien.NoRekamMedis
                            : null,

                    TanggalLahir =
                        a.Pasien != null
                            ? a.Pasien.TanggalLahir
                            : null,

                    a.LabBookingId,

                    NoOrderLab =
                        a.LabBooking != null
                            ? a.LabBooking.NoOrder
                            : null,

                    a.MappingBakteriId,

                    BakteriId =
                        a.MappingBakteri != null
                            ? a.MappingBakteri.BakteriId
                            : null,

                    SubBakteriId =
                        a.MappingBakteri != null
                            ? a.MappingBakteri.SubBakteriId
                            : null,

                    a.Keterangan,

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

            var totalRows = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(
                totalRows / (double)perPage);

            var listdata = await query
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
                from a in _applicationDbContext.LabHasilBakteris

                join user in _applicationDbContext.UserActives
                    on a.CreateBy equals user.UserActiveId into userJoin

                from u in userJoin.DefaultIfEmpty()

                where
                    a.LabHasilBakteriId == id
                    &&
                    (a.IsDelete == false ||
                     a.IsDelete == null)

                select new
                {
                    a.LabHasilBakteriId,

                    // ==========================
                    // LAB HASIL
                    // ==========================

                    a.LabHasilId,

                    // ==========================
                    // KUNJUNGAN
                    // ==========================

                    a.KunjunganId,

                    NoRegistrasi =
                        a.Kunjungan != null
                            ? a.Kunjungan.NoRegistrasi
                            : null,

                    JenisKunjungan =
                        a.Kunjungan != null
                            ? a.Kunjungan.JenisKunjungan
                            : null,

                    AsalKunjungan =
                        a.Kunjungan != null
                            ? a.Kunjungan.AsalKunjungan
                            : null,

                    // ==========================
                    // PASIEN
                    // ==========================

                    a.PasienId,

                    NamaPasien =
                        a.Pasien != null
                            ? a.Pasien.NamaLengkap
                            : null,

                    NoRM =
                        a.Pasien != null
                            ? a.Pasien.NoRekamMedis
                            : null,

                    TanggalLahir =
                        a.Pasien != null
                            ? a.Pasien.TanggalLahir
                            : null,

                    // ==========================
                    // LAB BOOKING
                    // ==========================

                    a.LabBookingId,

                    NoOrderLab =
                        a.LabBooking != null
                            ? a.LabBooking.NoOrder
                            : null,

                    // ==========================
                    // MAPPING BAKTERI
                    // ==========================

                    a.MappingBakteriId,

                    BakteriId =
                        a.MappingBakteri != null
                            ? a.MappingBakteri.BakteriId
                            : null,

                    SubBakteriId =
                        a.MappingBakteri != null
                            ? a.MappingBakteri.SubBakteriId
                            : null,

                    // ==========================
                    // LAINNYA
                    // ==========================

                    a.Keterangan,

                    a.CreateDateTime,
                    a.CreateBy,

                    CreateByName =
                        u != null
                            ? u.FullName
                            : null,

                    a.UpdateDateTime,
                    a.UpdateBy,

                    // ==========================
                    // DETAIL BAKTERI
                    // ==========================

                    LabDetailBakteris =
                        a.LabDetailBakteris
                            .Where(d =>
                                d.IsDelete == false ||
                                d.IsDelete == null)
                            .Select(d => new
                            {
                                d.LabDetailBakteriId,
                                d.AntibiotikId,

                                NamaAntibiotik =
                                    d.Antibiotik != null
                                        ? d.Antibiotik.NamaAntibiotik
                                        : null,

                                Microgram =
                                    d.Antibiotik != null
                                        ? d.Antibiotik.Microgram
                                        : null,

                                d.RangeZona,
                                d.ZonaMM,
                                d.ResultAntibiotik,
                                d.Keterangan
                            })
                            .ToList()
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
            [FromBody] LabHasilBakteriViewModel vm)
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
                // Cek koneksi database
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

                // Ambil email dari JWT
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

                // Ambil UserActive
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

                // =====================================
                // VALIDASI FOREIGN KEY
                // =====================================

                if (vm.LabHasilId.HasValue)
                {
                    var exists =
                        await _applicationDbContext
                            .LabHasils
                            .AnyAsync(x =>
                                x.HasilLabId ==
                                vm.LabHasilId.Value);

                    if (!exists)
                    {
                        return BadRequest(new
                        {
                            message =
                                "LabHasilId tidak ditemukan."
                        });
                    }
                }

                if (vm.KunjunganId.HasValue)
                {
                    var exists =
                        await _applicationDbContext
                            .Kunjungans
                            .AnyAsync(x =>
                                x.KunjunganID ==
                                vm.KunjunganId.Value);

                    if (!exists)
                    {
                        return BadRequest(new
                        {
                            message =
                                "KunjunganId tidak ditemukan."
                        });
                    }
                }

                if (vm.LabBookingId.HasValue)
                {
                    var exists =
                        await _applicationDbContext
                            .LabBookings
                            .AnyAsync(x =>
                                x.BookingLabId ==
                                vm.LabBookingId.Value);

                    if (!exists)
                    {
                        return BadRequest(new
                        {
                            message =
                                "LabBookingId tidak ditemukan."
                        });
                    }
                }

                if (vm.MappingBakteriId.HasValue)
                {
                    var exists =
                        await _applicationDbContext
                            .MapBakteris
                            .AnyAsync(x =>
                                x.MapBakteriId ==
                                vm.MappingBakteriId.Value);

                    if (!exists)
                    {
                        return BadRequest(new
                        {
                            message =
                                "MappingBakteriId tidak ditemukan."
                        });
                    }
                }

                // =====================================
                // CREATE
                // =====================================

                var data = new LabHasilBakteri
                {
                    LabHasilBakteriId =
                        Guid.NewGuid(),

                    LabHasilId =
                        vm.LabHasilId,

                    KunjunganId =
                        vm.KunjunganId,

                    PasienId =
                        vm.PasienId,

                    LabBookingId =
                        vm.LabBookingId,

                    MappingBakteriId =
                        vm.MappingBakteriId,

                    Keterangan =
                        vm.Keterangan,

                    CreateBy =
                        userActiveId,

                    CreateDateTime =
                        DateTimeOffset.UtcNow,

                    IsDelete =
                        false
                };

                _applicationDbContext
                    .LabHasilBakteris
                    .Add(data);

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
                            data.LabHasilBakteriId
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
                        $"Gagal menyimpan data: " +
                        $"{dbEx.InnerException?.Message ?? dbEx.Message}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message =
                        $"Terjadi kesalahan internal: {ex.Message}"
                });
            }
        }

        // ============================================================
        // UPDATE
        // ============================================================

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] LabHasilBakteriViewModel vm)
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
                        .LabHasilBakteris
                        .FirstOrDefaultAsync(x =>
                            x.LabHasilBakteriId == id &&
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

                // =====================================
                // VALIDASI MAPPING BAKTERI
                // =====================================

                if (vm.MappingBakteriId.HasValue)
                {
                    var exists =
                        await _applicationDbContext
                            .MapBakteris
                            .AnyAsync(x =>
                                x.MapBakteriId ==
                                vm.MappingBakteriId.Value);

                    if (!exists)
                    {
                        return BadRequest(new
                        {
                            message =
                                "MappingBakteriId tidak ditemukan."
                        });
                    }
                }

                // =====================================
                // UPDATE
                // =====================================

                data.LabHasilId =
                    vm.LabHasilId;

                data.KunjunganId =
                    vm.KunjunganId;

                data.PasienId =
                    vm.PasienId;

                data.LabBookingId =
                    vm.LabBookingId;

                data.MappingBakteriId =
                    vm.MappingBakteriId;

                data.Keterangan =
                    vm.Keterangan;

                data.UpdateBy =
                    userActiveId;

                data.UpdateDateTime =
                    DateTimeOffset.UtcNow;

                _applicationDbContext
                    .LabHasilBakteris
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
                        $"Gagal menyimpan data: " +
                        $"{dbEx.InnerException?.Message ?? dbEx.Message}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message =
                        $"Terjadi kesalahan internal: {ex.Message}"
                });
            }
        }

        // ============================================================
        // DELETE / SOFT DELETE
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
                        .LabHasilBakteris
                        .FirstOrDefaultAsync(x =>
                            x.LabHasilBakteriId == id);

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
                    .LabHasilBakteris
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
                        $"Gagal menghapus data: " +
                        $"{dbEx.InnerException?.Message ?? dbEx.Message}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message =
                        $"Terjadi kesalahan internal: {ex.Message}"
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
                from a in _applicationDbContext.LabHasilBakteris

                join user in _applicationDbContext.UserActives
                    on a.CreateBy equals user.UserActiveId into userJoin

                from u in userJoin.DefaultIfEmpty()

                where a.IsDelete == false ||
                      a.IsDelete == null

                select new
                {
                    a.CreateDateTime,
                    a.CreateBy,

                    CreateByName =
                        u != null
                            ? u.FullName
                            : null,

                    a.LabHasilBakteriId,

                    a.LabHasilId,

                    a.KunjunganId,

                    NoRegistrasi =
                        a.Kunjungan != null
                            ? a.Kunjungan.NoRegistrasi
                            : null,

                    JenisKunjungan =
                        a.Kunjungan != null
                            ? a.Kunjungan.JenisKunjungan
                            : null,

                    a.PasienId,

                    NamaPasien =
                        a.Pasien != null
                            ? a.Pasien.NamaLengkap
                            : null,

                    NoRM =
                        a.Pasien != null
                            ? a.Pasien.NoRekamMedis
                            : null,

                    a.LabBookingId,

                    NoOrderLab =
                        a.LabBooking != null
                            ? a.LabBooking.NoOrder
                            : null,

                    a.MappingBakteriId,

                    BakteriId =
                        a.MappingBakteri != null
                            ? a.MappingBakteri.BakteriId
                            : null,

                    SubBakteriId =
                        a.MappingBakteri != null
                            ? a.MappingBakteri.SubBakteriId
                            : null,

                    a.Keterangan,

                    a.UpdateDateTime,
                    a.UpdateBy
                };

            // =====================================
            // SEARCH
            // =====================================

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchPattern =
                    $"%{search.Trim()}%";

                query = query.Where(x =>

                    EF.Functions.ILike(
                        x.NoRegistrasi ?? "",
                        searchPattern)

                    ||

                    EF.Functions.ILike(
                        x.NoOrderLab ?? "",
                        searchPattern)

                    ||

                    EF.Functions.ILike(
                        x.NamaPasien ?? "",
                        searchPattern)

                    ||

                    EF.Functions.ILike(
                        x.NoRM ?? "",
                        searchPattern)

                    ||

                    EF.Functions.ILike(
                        x.Keterangan ?? "",
                        searchPattern)
                );
            }

            // =====================================
            // FILTER TANGGAL
            // =====================================

            if (startDate.HasValue &&
                endDate.HasValue)
            {
                DateTimeOffset startUtc =
                    startDate.Value
                        .Date
                        .ToUniversalTime();

                DateTimeOffset endUtc =
                    endDate.Value
                        .Date
                        .AddDays(1)
                        .AddTicks(-1)
                        .ToUniversalTime();

                query = query.Where(x =>
                    x.CreateDateTime >= startUtc &&
                    x.CreateDateTime <= endUtc);
            }

            // =====================================
            // FILTER PERIODE
            // =====================================

            if (periode.HasValue)
            {
                DateTime today =
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

            // =====================================
            // SORTING
            // =====================================

            query =
                sortDirection?.ToLower() == "desc"
                    ? orderBy switch
                    {
                        "CreateDateTime" =>
                            query.OrderByDescending(
                                x => x.CreateDateTime),

                        "CreateByName" =>
                            query.OrderByDescending(
                                x => x.CreateByName),

                        "NoOrderLab" =>
                            query.OrderByDescending(
                                x => x.NoOrderLab),

                        "NoRegistrasi" =>
                            query.OrderByDescending(
                                x => x.NoRegistrasi),

                        "NamaPasien" =>
                            query.OrderByDescending(
                                x => x.NamaPasien),

                        _ =>
                            query.OrderByDescending(
                                x => x.CreateDateTime)
                    }
                    : orderBy switch
                    {
                        "CreateDateTime" =>
                            query.OrderBy(
                                x => x.CreateDateTime),

                        "CreateByName" =>
                            query.OrderBy(
                                x => x.CreateByName),

                        "NoOrderLab" =>
                            query.OrderBy(
                                x => x.NoOrderLab),

                        "NoRegistrasi" =>
                            query.OrderBy(
                                x => x.NoRegistrasi),

                        "NamaPasien" =>
                            query.OrderBy(
                                x => x.NamaPasien),

                        _ =>
                            query.OrderBy(
                                x => x.CreateDateTime)
                    };

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