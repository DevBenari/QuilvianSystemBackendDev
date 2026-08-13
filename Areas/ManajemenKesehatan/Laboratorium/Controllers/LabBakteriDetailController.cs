using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
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
    public class LabBakteriDetailController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly ILogger<LabBakteriDetailController> _logger;

        public LabBakteriDetailController(
            ApplicationDbContext applicationDbContext,
            ILogger<LabBakteriDetailController> logger)
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
                from a in _applicationDbContext.Set<LabBakteriDetail>()

                join user in _applicationDbContext.UserActives
                    on a.CreateBy equals user.UserActiveId into userJoin

                from u in userJoin.DefaultIfEmpty()

                where a.IsDelete == false ||
                      a.IsDelete == null

                select new
                {
                    a.LabDetailBakteriId,

                    // ======================================
                    // LAB HASIL BAKTERI
                    // ======================================

                    a.LabHasilBakteriId,

                    LabBookingId =
                        a.LabHasilBakteri != null
                            ? a.LabHasilBakteri.LabBookingId
                            : null,

                    NoOrderLab =
                        a.LabHasilBakteri != null &&
                        a.LabHasilBakteri.LabBooking != null
                            ? a.LabHasilBakteri.LabBooking.NoOrder
                            : null,

                    MappingBakteriId =
                        a.LabHasilBakteri != null
                            ? a.LabHasilBakteri.MappingBakteriId
                            : null,

                    BakteriId =
                        a.LabHasilBakteri != null &&
                        a.LabHasilBakteri.MappingBakteri != null
                            ? a.LabHasilBakteri.MappingBakteri.BakteriId
                            : null,

                    SubBakteriId =
                        a.LabHasilBakteri != null &&
                        a.LabHasilBakteri.MappingBakteri != null
                            ? a.LabHasilBakteri.MappingBakteri.SubBakteriId
                            : null,

                    // ======================================
                    // KUNJUNGAN
                    // ======================================

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

                    // ======================================
                    // PASIEN
                    // ======================================

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

                    // ======================================
                    // ANTIBIOTIK
                    // ======================================

                    a.AntibiotikId,

                    KodeAntibiotik =
                        a.Antibiotik != null
                            ? a.Antibiotik.KodeAntibiotik
                            : null,

                    NamaAntibiotik =
                        a.Antibiotik != null
                            ? a.Antibiotik.NamaAntibiotik
                            : null,

                    Microgram =
                        a.Antibiotik != null
                            ? a.Antibiotik.Microgram
                            : null,

                    // ======================================
                    // HASIL
                    // ======================================

                    a.RangeZona,
                    a.ZonaMM,
                    a.ResultAntibiotik,
                    a.Keterangan,

                    // ======================================
                    // AUDIT
                    // ======================================

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
                from a in _applicationDbContext.Set<LabBakteriDetail>()

                join user in _applicationDbContext.UserActives
                    on a.CreateBy equals user.UserActiveId into userJoin

                from u in userJoin.DefaultIfEmpty()

                where
                    a.LabDetailBakteriId == id
                    &&
                    (a.IsDelete == false ||
                     a.IsDelete == null)

                select new
                {
                    a.LabDetailBakteriId,

                    // ======================================
                    // LAB HASIL BAKTERI
                    // ======================================

                    a.LabHasilBakteriId,

                    LabHasilId =
                        a.LabHasilBakteri != null
                            ? a.LabHasilBakteri.LabHasilId
                            : null,

                    LabBookingId =
                        a.LabHasilBakteri != null
                            ? a.LabHasilBakteri.LabBookingId
                            : null,

                    NoOrderLab =
                        a.LabHasilBakteri != null &&
                        a.LabHasilBakteri.LabBooking != null
                            ? a.LabHasilBakteri.LabBooking.NoOrder
                            : null,

                    MappingBakteriId =
                        a.LabHasilBakteri != null
                            ? a.LabHasilBakteri.MappingBakteriId
                            : null,

                    BakteriId =
                        a.LabHasilBakteri != null &&
                        a.LabHasilBakteri.MappingBakteri != null
                            ? a.LabHasilBakteri.MappingBakteri.BakteriId
                            : null,

                    SubBakteriId =
                        a.LabHasilBakteri != null &&
                        a.LabHasilBakteri.MappingBakteri != null
                            ? a.LabHasilBakteri.MappingBakteri.SubBakteriId
                            : null,

                    // ======================================
                    // KUNJUNGAN
                    // ======================================

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

                    // ======================================
                    // PASIEN
                    // ======================================

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

                    // ======================================
                    // ANTIBIOTIK
                    // ======================================

                    a.AntibiotikId,

                    KodeAntibiotik =
                        a.Antibiotik != null
                            ? a.Antibiotik.KodeAntibiotik
                            : null,

                    NamaAntibiotik =
                        a.Antibiotik != null
                            ? a.Antibiotik.NamaAntibiotik
                            : null,

                    Microgram =
                        a.Antibiotik != null
                            ? a.Antibiotik.Microgram
                            : null,

                    // ======================================
                    // HASIL PEMERIKSAAN
                    // ======================================

                    a.RangeZona,
                    a.ZonaMM,
                    a.ResultAntibiotik,
                    a.Keterangan,

                    // ======================================
                    // AUDIT
                    // ======================================

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
            [FromBody] LabBakteriDetailViewModel vm)
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
                // ======================================
                // CEK DATABASE
                // ======================================

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

                // ======================================
                // USER LOGIN
                // ======================================

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

                // ======================================
                // VALIDASI LAB HASIL BAKTERI
                // ======================================

                if (!vm.LabHasilBakteriId.HasValue)
                {
                    return BadRequest(new
                    {
                        message =
                            "LabHasilBakteriId wajib diisi."
                    });
                }

                var labHasilBakteriExists =
                    await _applicationDbContext
                        .Set<LabHasilBakteri>()
                        .AnyAsync(x =>
                            x.LabHasilBakteriId ==
                            vm.LabHasilBakteriId.Value
                            &&
                            (x.IsDelete == false ||
                             x.IsDelete == null));

                if (!labHasilBakteriExists)
                {
                    return BadRequest(new
                    {
                        message =
                            "LabHasilBakteriId tidak ditemukan."
                    });
                }

                // ======================================
                // VALIDASI ANTIBIOTIK
                // ======================================

                if (vm.AntibiotikId == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        message =
                            "AntibiotikId wajib diisi."
                    });
                }

                var antibiotikExists =
                    await _applicationDbContext
                        .Set<MstAntibiotik>()
                        .AnyAsync(x =>
                            x.AntibiotikId ==
                            vm.AntibiotikId
                            &&
                            (x.IsDelete == false ||
                             x.IsDelete == null));

                if (!antibiotikExists)
                {
                    return BadRequest(new
                    {
                        message =
                            "AntibiotikId tidak ditemukan."
                    });
                }

                // ======================================
                // CEK DUPLIKASI ANTIBIOTIK
                // ======================================

                var isDuplicate =
                    await _applicationDbContext
                        .Set<LabBakteriDetail>()
                        .AnyAsync(x =>
                            x.LabHasilBakteriId ==
                            vm.LabHasilBakteriId
                            &&
                            x.AntibiotikId ==
                            vm.AntibiotikId
                            &&
                            (x.IsDelete == false ||
                             x.IsDelete == null));

                if (isDuplicate)
                {
                    return Conflict(new
                    {
                        message =
                            "Antibiotik tersebut sudah tersedia pada hasil bakteri ini."
                    });
                }

                // ======================================
                // CREATE
                // ======================================

                var data = new LabBakteriDetail
                {
                    LabDetailBakteriId =
                        Guid.NewGuid(),

                    LabHasilBakteriId =
                        vm.LabHasilBakteriId,

                    KunjunganId =
                        vm.KunjunganId,

                    PasienId =
                        vm.PasienId,

                    AntibiotikId =
                        vm.AntibiotikId,

                    RangeZona =
                        vm.RangeZona,

                    ZonaMM =
                        vm.ZonaMM,

                    ResultAntibiotik =
                        vm.ResultAntibiotik,

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
                    .Set<LabBakteriDetail>()
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
                            data.LabDetailBakteriId
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
            [FromBody] LabBakteriDetailViewModel vm)
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

                // ======================================
                // USER LOGIN
                // ======================================

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

                // ======================================
                // CARI DATA
                // ======================================

                var data =
                    await _applicationDbContext
                        .Set<LabBakteriDetail>()
                        .FirstOrDefaultAsync(x =>
                            x.LabDetailBakteriId == id
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

                // ======================================
                // VALIDASI LAB HASIL BAKTERI
                // ======================================

                if (!vm.LabHasilBakteriId.HasValue)
                {
                    return BadRequest(new
                    {
                        message =
                            "LabHasilBakteriId wajib diisi."
                    });
                }

                var labHasilBakteriExists =
                    await _applicationDbContext
                        .Set<LabHasilBakteri>()
                        .AnyAsync(x =>
                            x.LabHasilBakteriId ==
                            vm.LabHasilBakteriId.Value
                            &&
                            (x.IsDelete == false ||
                             x.IsDelete == null));

                if (!labHasilBakteriExists)
                {
                    return BadRequest(new
                    {
                        message =
                            "LabHasilBakteriId tidak ditemukan."
                    });
                }

                // ======================================
                // VALIDASI ANTIBIOTIK
                // ======================================

                if (vm.AntibiotikId == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        message =
                            "AntibiotikId wajib diisi."
                    });
                }

                var antibiotikExists =
                    await _applicationDbContext
                        .Set<MstAntibiotik>()
                        .AnyAsync(x =>
                            x.AntibiotikId ==
                            vm.AntibiotikId
                            &&
                            (x.IsDelete == false ||
                             x.IsDelete == null));

                if (!antibiotikExists)
                {
                    return BadRequest(new
                    {
                        message =
                            "AntibiotikId tidak ditemukan."
                    });
                }

                // ======================================
                // DUPLIKASI
                // ======================================

                var isDuplicate =
                    await _applicationDbContext
                        .Set<LabBakteriDetail>()
                        .AnyAsync(x =>
                            x.LabDetailBakteriId != id
                            &&
                            x.LabHasilBakteriId ==
                            vm.LabHasilBakteriId
                            &&
                            x.AntibiotikId ==
                            vm.AntibiotikId
                            &&
                            (x.IsDelete == false ||
                             x.IsDelete == null));

                if (isDuplicate)
                {
                    return Conflict(new
                    {
                        message =
                            "Antibiotik tersebut sudah tersedia pada hasil bakteri ini."
                    });
                }

                // ======================================
                // UPDATE
                // ======================================

                data.LabHasilBakteriId =
                    vm.LabHasilBakteriId;

                data.KunjunganId =
                    vm.KunjunganId;

                data.PasienId =
                    vm.PasienId;

                data.AntibiotikId =
                    vm.AntibiotikId;

                data.RangeZona =
                    vm.RangeZona;

                data.ZonaMM =
                    vm.ZonaMM;

                data.ResultAntibiotik =
                    vm.ResultAntibiotik;

                data.Keterangan =
                    vm.Keterangan;

                data.UpdateBy =
                    userActiveId;

                data.UpdateDateTime =
                    DateTimeOffset.UtcNow;

                _applicationDbContext
                    .Set<LabBakteriDetail>()
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

                // ======================================
                // USER LOGIN
                // ======================================

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

                // ======================================
                // CARI DATA
                // ======================================

                var data =
                    await _applicationDbContext
                        .Set<LabBakteriDetail>()
                        .FirstOrDefaultAsync(x =>
                            x.LabDetailBakteriId == id);

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

                // ======================================
                // SOFT DELETE
                // ======================================

                data.DeleteBy =
                    userActiveId;

                data.DeleteDateTime =
                    DateTimeOffset.UtcNow;

                data.IsDelete =
                    true;

                _applicationDbContext
                    .Set<LabBakteriDetail>()
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
                from a in _applicationDbContext.Set<LabBakteriDetail>()

                join user in _applicationDbContext.UserActives
                    on a.CreateBy equals user.UserActiveId into userJoin

                from u in userJoin.DefaultIfEmpty()

                where a.IsDelete == false ||
                      a.IsDelete == null

                select new
                {
                    a.LabDetailBakteriId,

                    a.LabHasilBakteriId,

                    NoOrderLab =
                        a.LabHasilBakteri != null &&
                        a.LabHasilBakteri.LabBooking != null
                            ? a.LabHasilBakteri.LabBooking.NoOrder
                            : null,

                    MappingBakteriId =
                        a.LabHasilBakteri != null
                            ? a.LabHasilBakteri.MappingBakteriId
                            : null,

                    BakteriId =
                        a.LabHasilBakteri != null &&
                        a.LabHasilBakteri.MappingBakteri != null
                            ? a.LabHasilBakteri.MappingBakteri.BakteriId
                            : null,

                    SubBakteriId =
                        a.LabHasilBakteri != null &&
                        a.LabHasilBakteri.MappingBakteri != null
                            ? a.LabHasilBakteri.MappingBakteri.SubBakteriId
                            : null,

                    a.KunjunganId,

                    NoRegistrasi =
                        a.Kunjungan != null
                            ? a.Kunjungan.NoRegistrasi
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

                    a.AntibiotikId,

                    KodeAntibiotik =
                        a.Antibiotik != null
                            ? a.Antibiotik.KodeAntibiotik
                            : null,

                    NamaAntibiotik =
                        a.Antibiotik != null
                            ? a.Antibiotik.NamaAntibiotik
                            : null,

                    Microgram =
                        a.Antibiotik != null
                            ? a.Antibiotik.Microgram
                            : null,

                    a.RangeZona,
                    a.ZonaMM,
                    a.ResultAntibiotik,
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
                var searchPattern =
                    $"%{search.Trim()}%";

                query = query.Where(x =>

                    EF.Functions.ILike(
                        x.NoOrderLab ?? "",
                        searchPattern)

                    ||

                    EF.Functions.ILike(
                        x.NoRegistrasi ?? "",
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
                        x.KodeAntibiotik ?? "",
                        searchPattern)

                    ||

                    EF.Functions.ILike(
                        x.NamaAntibiotik ?? "",
                        searchPattern)

                    ||

                    EF.Functions.ILike(
                        x.RangeZona ?? "",
                        searchPattern)

                    ||

                    EF.Functions.ILike(
                        x.ResultAntibiotik ?? "",
                        searchPattern)

                    ||

                    EF.Functions.ILike(
                        x.Keterangan ?? "",
                        searchPattern)
                );
            }

            // ========================================================
            // FILTER TANGGAL
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

                        "NoOrderLab" =>
                            query.OrderByDescending(
                                x => x.NoOrderLab),

                        "NoRegistrasi" =>
                            query.OrderByDescending(
                                x => x.NoRegistrasi),

                        "NamaPasien" =>
                            query.OrderByDescending(
                                x => x.NamaPasien),

                        "NamaAntibiotik" =>
                            query.OrderByDescending(
                                x => x.NamaAntibiotik),

                        "ZonaMM" =>
                            query.OrderByDescending(
                                x => x.ZonaMM),

                        "ResultAntibiotik" =>
                            query.OrderByDescending(
                                x => x.ResultAntibiotik),

                        _ =>
                            query.OrderByDescending(
                                x => x.CreateDateTime)
                    }
                    : orderBy switch
                    {
                        "CreateDateTime" =>
                            query.OrderBy(
                                x => x.CreateDateTime),

                        "NoOrderLab" =>
                            query.OrderBy(
                                x => x.NoOrderLab),

                        "NoRegistrasi" =>
                            query.OrderBy(
                                x => x.NoRegistrasi),

                        "NamaPasien" =>
                            query.OrderBy(
                                x => x.NamaPasien),

                        "NamaAntibiotik" =>
                            query.OrderBy(
                                x => x.NamaAntibiotik),

                        "ZonaMM" =>
                            query.OrderBy(
                                x => x.ZonaMM),

                        "ResultAntibiotik" =>
                            query.OrderBy(
                                x => x.ResultAntibiotik),

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

            var rows = await query
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