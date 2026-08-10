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
    public class LabHasilSpecimenController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<LabHasilSpecimenController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public LabHasilSpecimenController(
        ApplicationDbContext applicationDbContext,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<LabHasilSpecimenController> logger,
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
            var listdata = (from a in _applicationDbContext.LabHasilSpecimens
                            join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                            on a.CreateBy equals u.UserActiveId
                            where a.IsDelete == false && a.LabHasilSpecimenId == id
                            select new
                            {
                                a.CreateDateTime,
                                a.CreateBy,
                                CreateByName = u.FullName,
                                a.LabHasilSpecimenId,
                                a.LabHasilId,
                                a.KunjunganId,
                                JenisKunjungan = a.Kunjungan != null ? a.Kunjungan.JenisKunjungan : null,
                                AsalKunjungan = a.Kunjungan != null ? a.Kunjungan.AsalKunjungan : null,
                                NoRegistrasi = a.Kunjungan != null ? a.Kunjungan.NoRegistrasi : null,
                                a.PasienId,
                                NamaPasien = a.Pasien != null ? a.Pasien.NamaLengkap : null,
                                NoRM = a.Pasien != null ? a.Pasien.NoRekamMedis : null,
                                TanggalLahir = a.Pasien != null ? a.Pasien.TanggalLahir : null,
                                a.AsalSpecimenId,
                                NamaAsalSpecimen = a.AsalSpecimen != null ? a.AsalSpecimen.AsalSpecimen : null,
                                JenisSpecimen = a.JenisSpecimens.Select(j => new
                                {
                                    j.JenisSpecimenId,

                                    NamaJenisSpecimen =
                                        j.JenisSpecimen != null
                                            ? j.JenisSpecimen.NamaJenisSpecimen
                                            : null
                                })
                                .ToList(),
                            });

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
        public async Task<IActionResult> Create([FromBody] LabHasilSpecimenViewModel vm)
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
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new
                    {
                        message = "Tidak dapat terhubung ke database."
                    });
                }

                // Ambil user login dari JWT
                var emailLogin = User
                    .FindFirst(ClaimTypes.NameIdentifier)?
                    .Value;

                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new
                    {
                        message = "User tidak terautentikasi!"
                    });
                }

                // Ambil UserActive
                var getUserActive = await _applicationDbContext
                    .UserActives
                    .FirstOrDefaultAsync(x =>
                        x.Email == emailLogin);

                if (getUserActive == null)
                {
                    return Unauthorized(new
                    {
                        message = "User aktif tidak ditemukan!"
                    });
                }

                var userActiveId = getUserActive.UserActiveId;

                // Validasi Jenis Specimen
                if (vm.JenisSpecimenId == null ||
                    !vm.JenisSpecimenId.Any())
                {
                    return BadRequest(new
                    {
                        message = "Jenis specimen wajib dipilih."
                    });
                }

                // Hilangkan duplicate ID
                var jenisSpecimenIds = vm.JenisSpecimenId
                    .Where(x => x != Guid.Empty)
                    .Distinct()
                    .ToList();

                if (!jenisSpecimenIds.Any())
                {
                    return BadRequest(new
                    {
                        message = "Jenis specimen tidak valid."
                    });
                }

                // Validasi semua JenisSpecimenId tersedia di database
                var validJenisSpecimenIds = await _applicationDbContext
                    .SpecimenJeniss
                    .Where(x =>
                        jenisSpecimenIds.Contains(x.JenisSpecimenId))
                    .Select(x => x.JenisSpecimenId)
                    .ToListAsync();

                if (validJenisSpecimenIds.Count != jenisSpecimenIds.Count)
                {
                    var invalidIds = jenisSpecimenIds
                        .Except(validJenisSpecimenIds)
                        .ToList();

                    return BadRequest(new
                    {
                        message = "Terdapat JenisSpecimenId yang tidak ditemukan.",
                        invalidJenisSpecimenIds = invalidIds
                    });
                }

                var labHasilSpecimenId = Guid.NewGuid();

                // Buat data utama
                var data = new LabHasilSpecimen
                {
                    LabHasilSpecimenId = labHasilSpecimenId,

                    LabHasilId = vm.LabHasilId,
                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,
                    AsalSpecimenId = vm.AsalSpecimenId,

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,

                    // Navigation collection
                    JenisSpecimens = jenisSpecimenIds
                        .Select(jenisSpecimenId =>
                            new LabHasilSpecimenJenis
                            {
                                LabHasilSpecimenJenisId = Guid.NewGuid(),

                                LabHasilSpecimenId = labHasilSpecimenId,

                                JenisSpecimenId = jenisSpecimenId
                            })
                        .ToList()
                };

                // Simpan data utama + relation
                await _applicationDbContext
                    .LabHasilSpecimens
                    .AddAsync(data);

                var result = await _applicationDbContext
                    .SaveChangesAsync();

                if (result <= 0)
                {
                    return StatusCode(500, new
                    {
                        message = "Data tidak berhasil disimpan ke database."
                    });
                }

                return Created("", new
                {
                    message = "Tambah Data Berhasil || 201 Created",

                    data = new
                    {
                        labHasilSpecimenId = data.LabHasilSpecimenId,

                        jenisSpecimenIds = jenisSpecimenIds
                    }
                });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new
                {
                    message = "Gagal menyimpan data.",
                    detail = dbEx.InnerException?.Message
                             ?? dbEx.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Terjadi kesalahan internal.",
                    detail = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id,[FromBody] LabHasilSpecimenViewModel vm)
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
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new
                    {
                        message = "Tidak dapat terhubung ke database."
                    });
                }

                // Ambil user login dari JWT
                var emailLogin = User
                    .FindFirst(ClaimTypes.NameIdentifier)?
                    .Value;

                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new
                    {
                        message = "User tidak terautentikasi!"
                    });
                }

                // Ambil UserActive
                var getUserActive = await _applicationDbContext
                    .UserActives
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Email == emailLogin);

                if (getUserActive == null)
                {
                    return Unauthorized(new
                    {
                        message = "User aktif tidak ditemukan!"
                    });
                }

                var userActiveId = getUserActive.UserActiveId;

                // Ambil data LabHasilSpecimen beserta relation JenisSpecimen
                var data = await _applicationDbContext
                    .LabHasilSpecimens
                    .Include(x => x.JenisSpecimens)
                    .FirstOrDefaultAsync(x =>
                        x.LabHasilSpecimenId == id);

                if (data == null)
                {
                    return NotFound(new
                    {
                        message = "Data Lab Hasil Specimen tidak ditemukan."
                    });
                }

                // Validasi Jenis Specimen
                if (vm.JenisSpecimenId == null ||
                    !vm.JenisSpecimenId.Any())
                {
                    return BadRequest(new
                    {
                        message = "Jenis specimen wajib dipilih."
                    });
                }

                // Bersihkan Guid.Empty dan duplicate
                var jenisSpecimenIds = vm.JenisSpecimenId
                    .Where(x => x != Guid.Empty)
                    .Distinct()
                    .ToList();

                if (!jenisSpecimenIds.Any())
                {
                    return BadRequest(new
                    {
                        message = "Jenis specimen tidak valid."
                    });
                }

                // Validasi semua JenisSpecimenId tersedia di database
                var validJenisSpecimenIds = await _applicationDbContext
                    .SpecimenJeniss
                    .Where(x =>
                        jenisSpecimenIds.Contains(x.JenisSpecimenId))
                    .Select(x => x.JenisSpecimenId)
                    .ToListAsync();

                if (validJenisSpecimenIds.Count != jenisSpecimenIds.Count)
                {
                    var invalidIds = jenisSpecimenIds
                        .Except(validJenisSpecimenIds)
                        .ToList();

                    return BadRequest(new
                    {
                        message = "Terdapat JenisSpecimenId yang tidak ditemukan.",
                        invalidJenisSpecimenIds = invalidIds
                    });
                }

                // ============================================
                // UPDATE DATA UTAMA
                // ============================================

                data.LabHasilId = vm.LabHasilId;
                data.KunjunganId = vm.KunjunganId;
                data.PasienId = vm.PasienId;
                data.AsalSpecimenId = vm.AsalSpecimenId;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                // ============================================
                // HAPUS RELATION JENIS SPECIMEN LAMA
                // ============================================

                if (data.JenisSpecimens != null &&
                    data.JenisSpecimens.Any())
                {
                    _applicationDbContext
                        .Set<LabHasilSpecimenJenis>()
                        .RemoveRange(data.JenisSpecimens);

                    data.JenisSpecimens.Clear();
                }

                // ============================================
                // TAMBAHKAN RELATION JENIS SPECIMEN BARU
                // ============================================

                foreach (var jenisSpecimenId in jenisSpecimenIds)
                {
                    data.JenisSpecimens.Add(
                        new LabHasilSpecimenJenis
                        {
                            LabHasilSpecimenJenisId = Guid.NewGuid(),

                            LabHasilSpecimenId =
                                data.LabHasilSpecimenId,

                            JenisSpecimenId =
                                jenisSpecimenId
                        });
                }

                // Simpan perubahan
                var result = await _applicationDbContext
                    .SaveChangesAsync();

                if (result <= 0)
                {
                    return StatusCode(500, new
                    {
                        message = "Data tidak berhasil diperbarui."
                    });
                }

                return Ok(new
                {
                    message = "Update Data Berhasil || 200 OK",

                    data = new
                    {
                        labHasilSpecimenId =
                            data.LabHasilSpecimenId,

                        labHasilId =
                            data.LabHasilId,

                        kunjunganId =
                            data.KunjunganId,

                        pasienId =
                            data.PasienId,

                        asalSpecimenId =
                            data.AsalSpecimenId,

                        jenisSpecimenIds =
                            jenisSpecimenIds,

                        updateBy =
                            data.UpdateBy,

                        updateDateTime =
                            data.UpdateDateTime
                    }
                });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new
                {
                    message = "Gagal memperbarui data.",
                    detail = dbEx.InnerException?.Message
                             ?? dbEx.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Terjadi kesalahan internal.",
                    detail = ex.Message
                });
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
                var data = await _applicationDbContext.LabHasilSpecimens.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.LabHasilSpecimens.Update(data);
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

            // Query data
            var query = (from a in _applicationDbContext.LabHasilSpecimens
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.LabHasilSpecimenId,
                             a.LabHasilId,
                             a.KunjunganId,
                             JenisKunjungan = a.Kunjungan != null ? a.Kunjungan.JenisKunjungan : null,
                             AsalKunjungan = a.Kunjungan != null ? a.Kunjungan.AsalKunjungan : null,
                             NoRegistrasi = a.Kunjungan != null ? a.Kunjungan.NoRegistrasi : null,
                             a.PasienId,
                             NamaPasien = a.Pasien != null ? a.Pasien.NamaLengkap : null,
                             TanggalLahir = a.Pasien != null ? a.Pasien.TanggalLahir : null,
                             NoRM = a.Pasien != null ? a.Pasien.NoRekamMedis : null,
                             a.AsalSpecimenId,
                             NamaAsalSpecimen = a.AsalSpecimen != null ? a.AsalSpecimen.AsalSpecimen : null,
                             JenisSpecimen = a.JenisSpecimens.Select(j => new
                             {
                                 j.JenisSpecimenId,

                                 NamaJenisSpecimen =
                                     j.JenisSpecimen != null
                                         ? j.JenisSpecimen.NamaJenisSpecimen
                                         : null
                             })
                                .ToList(),
                         });

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                query = query.Where(u =>
                    EF.Functions.ILike(u.NamaPasien, search) ||
                    EF.Functions.ILike(u.NoRM, search) ||
                    EF.Functions.ILike(u.NoRegistrasi, search)
                );
            }

            //// **Filter berdasarkan tanggal**
            if (startDate.HasValue && endDate.HasValue)
            {
                DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                query = query.Where(u =>
                    u.CreateDateTime >= startUtc &&
                    u.CreateDateTime <= endUtc);
            }

            // Filter berdasarkan periode (Hari Ini, Minggu Ini, dll) hanya jika periode memiliki nilai
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
                            u.CreateDateTime.Date <= today
                        );
                        break;
                    case PeriodeFilter.LastWeek:
                        query = query.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                            u.CreateDateTime.Date < today.AddDays(-(int)today.DayOfWeek)
                        );
                        break;
                    case PeriodeFilter.ThisMonth:
                        query = query.Where(u =>
                            u.CreateDateTime.Month == today.Month &&
                            u.CreateDateTime.Year == today.Year
                        );
                        break;
                    case PeriodeFilter.LastMonth:
                        query = query.Where(u =>
                            u.CreateDateTime.Month == today.Month - 1 &&
                            u.CreateDateTime.Year == today.Year
                        );
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

            // Sorting Data dengan cara yang lebih aman
            query = sortDirection?.ToLower() == "desc"
                ? orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderBy(u => u.CreateDateTime),
                    "CreateByName" => query.OrderBy(u => u.CreateByName),
                    _ => query.OrderBy(u => u.CreateDateTime)
                };

            // Pagination
            var totalRows = query.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);
            var rows = query.Skip((page - 1) * perPage).Take(perPage).ToList();

            if (rows.Count == 0 && page > totalPages)
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
