using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class LabHasilSpecimenJenisController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<LabHasilSpecimenJenisController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public LabHasilSpecimenJenisController(
        ApplicationDbContext applicationDbContext,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<LabHasilSpecimenJenisController> logger,
        IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        // ============================================================
        // GET ALL / PAGED
        // ============================================================
        [HttpGet("paged")]
        public async Task<IActionResult> Paged(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "JenisSpecimen",
            string? sortDirection = "asc",
            CancellationToken ct = default)
        {
            if (page < 1)
                page = 1;

            if (perPage < 1)
                perPage = 10;

            if (perPage > 100)
                perPage = 100;


            var query = _applicationDbContext.LabHasilSpecimenJenis
                .AsNoTracking()
                .Select(x => new
                {
                    x.LabHasilSpecimenJenisId,
                    x.LabHasilSpecimenId,
                    x.JenisSpecimenId,

                    // ==========================================
                    // NAVIGATION LAB HASIL SPECIMEN
                    // ==========================================
                    LabHasilSpecimen = x.LabHasilSpecimen == null
                        ? null
                        : new
                        {
                            x.LabHasilSpecimen.LabHasilSpecimenId

                            // Kalau property berikut memang ada
                            // di model LabHasilSpecimen,
                            // dapat ditambahkan:
                            //
                            // x.LabHasilSpecimen.LabHasilId,
                            // x.LabHasilSpecimen.KunjunganId,
                            // x.LabHasilSpecimen.PasienId,
                            // x.LabHasilSpecimen.AsalSpecimenId,
                            // x.LabHasilSpecimen.Keterangan
                        },


                    // ==========================================
                    // NAVIGATION JENIS SPECIMEN
                    // ==========================================
                    JenisSpecimen = x.JenisSpecimen == null
                        ? null
                        : new
                        {
                            x.JenisSpecimen.JenisSpecimenId,
                            x.JenisSpecimen.NamaJenisSpecimen
                        },


                    // Digunakan untuk search + sorting
                    NamaJenisSpecimen = x.JenisSpecimen != null
                        ? x.JenisSpecimen.NamaJenisSpecimen
                        : null
                });


            // ========================================================
            // SEARCH
            // ========================================================
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = $"%{search.Trim()}%";

                query = query.Where(x =>
                    x.NamaJenisSpecimen != null &&
                    EF.Functions.ILike(
                        x.NamaJenisSpecimen,
                        keyword));
            }


            // ========================================================
            // SORTING
            // ========================================================
            var descending = string.Equals(
                sortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase);


            query = (orderBy?.ToLower(), descending) switch
            {
                ("jenisspecimen", true) =>
                    query.OrderByDescending(x =>
                        x.NamaJenisSpecimen),

                ("jenisspecimen", false) =>
                    query.OrderBy(x =>
                        x.NamaJenisSpecimen),

                _ =>
                    query.OrderBy(x =>
                        x.NamaJenisSpecimen)
            };


            // ========================================================
            // PAGINATION
            // ========================================================
            var totalRows = await query.CountAsync(ct);

            var totalPages = totalRows == 0
                ? 0
                : (int)Math.Ceiling(
                    totalRows / (double)perPage);


            if (totalRows > 0 && page > totalPages)
            {
                return NotFound(new
                {
                    status = "error",
                    message = "Page not found."
                });
            }


            var rows = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync(ct);


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



        // ============================================================
        // GET BY ID
        // ============================================================
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


            var data = await _applicationDbContext
                .LabHasilSpecimenJenis
                .AsNoTracking()
                .Where(x =>
                    x.LabHasilSpecimenJenisId == id)
                .Select(x => new
                {
                    // ==========================================
                    // DATA UTAMA
                    // ==========================================
                    x.LabHasilSpecimenJenisId,
                    x.LabHasilSpecimenId,
                    x.JenisSpecimenId,


                    // ==========================================
                    // LAB HASIL SPECIMEN
                    // ==========================================
                    LabHasilSpecimen = x.LabHasilSpecimen == null
                        ? null
                        : new
                        {
                            x.LabHasilSpecimen.LabHasilSpecimenId

                            // Tambahkan kalau memang tersedia
                            // di class LabHasilSpecimen:
                            //
                            // x.LabHasilSpecimen.LabHasilId,
                            // x.LabHasilSpecimen.KunjunganId,
                            // x.LabHasilSpecimen.PasienId,
                            // x.LabHasilSpecimen.AsalSpecimenId,
                            // x.LabHasilSpecimen.Keterangan
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
                        }
                })
                .FirstOrDefaultAsync(ct);


            if (data == null)
            {
                return NotFound(new
                {
                    status = "error",
                    message =
                        "Data Lab Hasil Specimen Jenis tidak ditemukan."
                });
            }


            return Ok(new
            {
                status = "success",
                message = "Data retrieved successfully",
                data
            });
        }

        // ============================================================
        // CREATE
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] LabHasilSpecimenJenisViewModel request,
            CancellationToken ct)
        {
            if (request == null)
            {
                return BadRequest(new
                {
                    status = "error",
                    message = "Data tidak boleh kosong."
                });
            }


            // ========================================================
            // VALIDASI LAB HASIL SPECIMEN
            // ========================================================
            if (request.LabHasilSpecimenId.HasValue)
            {
                var labHasilSpecimenExists =
                    await _applicationDbContext
                        .LabHasilSpecimens
                        .AsNoTracking()
                        .AnyAsync(x =>
                            x.LabHasilSpecimenId ==
                            request.LabHasilSpecimenId.Value,
                            ct);


                if (!labHasilSpecimenExists)
                {
                    return BadRequest(new
                    {
                        status = "error",
                        message =
                            "Lab Hasil Specimen tidak ditemukan."
                    });
                }
            }


            // ========================================================
            // VALIDASI JENIS SPECIMEN
            // ========================================================
            if (request.JenisSpecimenId.HasValue)
            {
                var jenisSpecimenExists =
                    await _applicationDbContext
                        .SpecimenJeniss
                        .AsNoTracking()
                        .AnyAsync(x =>
                            x.JenisSpecimenId ==
                            request.JenisSpecimenId.Value,
                            ct);


                if (!jenisSpecimenExists)
                {
                    return BadRequest(new
                    {
                        status = "error",
                        message =
                            "Jenis Specimen tidak ditemukan."
                    });
                }
            }

            // ========================================================
            // CREATE ENTITY
            // ========================================================
            var entity = new LabHasilSpecimenJenis
            {
                LabHasilSpecimenJenisId = Guid.NewGuid(),

                LabHasilSpecimenId =
                    request.LabHasilSpecimenId,

                JenisSpecimenId =
                    request.JenisSpecimenId
            };


            await _applicationDbContext
                .LabHasilSpecimenJenis
                .AddAsync(entity, ct);

            await _applicationDbContext
                .SaveChangesAsync(ct);


            return Ok(new
            {
                status = "success",
                message =
                    "Data Lab Hasil Specimen Jenis berhasil ditambahkan.",
                data = new
                {
                    entity.LabHasilSpecimenJenisId
                }
            });
        }



        // ============================================================
        // UPDATE
        // ============================================================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] LabHasilSpecimenJenisViewModel request,
            CancellationToken ct)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new
                {
                    status = "error",
                    message = "Parameter ID tidak valid."
                });
            }


            if (request == null)
            {
                return BadRequest(new
                {
                    status = "error",
                    message = "Data tidak boleh kosong."
                });
            }


            var entity = await _applicationDbContext
                .LabHasilSpecimenJenis
                .FirstOrDefaultAsync(x =>
                    x.LabHasilSpecimenJenisId == id,
                    ct);


            if (entity == null)
            {
                return NotFound(new
                {
                    status = "error",
                    message =
                        "Data Lab Hasil Specimen Jenis tidak ditemukan."
                });
            }


            // ========================================================
            // VALIDASI LAB HASIL SPECIMEN
            // ========================================================
            if (request.LabHasilSpecimenId.HasValue)
            {
                var exists =
                    await _applicationDbContext
                        .LabHasilSpecimens
                        .AsNoTracking()
                        .AnyAsync(x =>
                            x.LabHasilSpecimenId ==
                                request.LabHasilSpecimenId.Value,
                            ct);


                if (!exists)
                {
                    return BadRequest(new
                    {
                        status = "error",
                        message =
                            "Lab Hasil Specimen tidak ditemukan."
                    });
                }
            }


            // ========================================================
            // VALIDASI JENIS SPECIMEN
            // ========================================================
            if (request.JenisSpecimenId.HasValue)
            {
                var exists =
                    await _applicationDbContext
                        .SpecimenJeniss
                        .AsNoTracking()
                        .AnyAsync(x =>
                            x.JenisSpecimenId ==
                                request.JenisSpecimenId.Value,
                            ct);


                if (!exists)
                {
                    return BadRequest(new
                    {
                        status = "error",
                        message =
                            "Jenis Specimen tidak ditemukan."
                    });
                }
            }

            // ========================================================
            // UPDATE
            // ========================================================
            entity.LabHasilSpecimenId =
                request.LabHasilSpecimenId;

            entity.JenisSpecimenId =
                request.JenisSpecimenId;


            await _applicationDbContext
                .SaveChangesAsync(ct);


            return Ok(new
            {
                status = "success",
                message =
                    "Data Lab Hasil Specimen Jenis berhasil diperbarui."
            });
        }



        // ============================================================
        // DELETE
        // ============================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id,CancellationToken ct)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new
                {
                    status = "error",
                    message = "Parameter ID tidak valid."
                });
            }


            var entity = await _applicationDbContext
                .LabHasilSpecimenJenis
                .FirstOrDefaultAsync(x =>
                    x.LabHasilSpecimenJenisId == id,
                    ct);


            if (entity == null)
            {
                return NotFound(new
                {
                    status = "error",
                    message =
                        "Data Lab Hasil Specimen Jenis tidak ditemukan."
                });
            }


            _applicationDbContext
                .LabHasilSpecimenJenis
                .Remove(entity);

            await _applicationDbContext
                .SaveChangesAsync(ct);


            return Ok(new
            {
                status = "success",
                message =
                    "Data Lab Hasil Specimen Jenis berhasil dihapus."
            });
        }
    }


}

