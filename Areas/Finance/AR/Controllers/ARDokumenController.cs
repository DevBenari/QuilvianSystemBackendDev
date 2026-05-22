using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.Finance.AR.Models;
using QuilvianSystemBackendDev.Areas.Finance.AR.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace QuilvianSystemBackendDev.Areas.Finance.AR.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class ARDokumenController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<ARDokumenController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ARDokumenController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ARDokumenController> logger,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _applicationDbContext = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        // =====================================================
        // PAGED
        // =====================================================

        [HttpGet("paged")]
        public async Task<IActionResult> PagedARDokumen(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "TglTerimaDok",
            string? sortDirection = "desc",

            [FromQuery, SwaggerSchema(Format = "date-time")]
            DateTime? startDate = null,

            [FromQuery, SwaggerSchema(Format = "date-time")]
            DateTime? endDate = null
        )
        {
            try
            {
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new
                    {
                        message = "Tidak dapat terhubung ke database."
                    });
                }

                if (page < 1)
                    page = 1;

                if (perPage < 1)
                    perPage = 10;

                var query =
                    from d in _applicationDbContext.ARDokumens
                        .AsNoTracking()

                    join h in _applicationDbContext.ARHeaders
                        .AsNoTracking()
                    on d.ARHeaderId equals h.ARHeaderId

                    where h.IsDelete == false

                    select new
                    {
                        d.ARDokumenId,
                        d.ARHeaderId,
                        d.KunjunganId,
                        d.PasienId,

                        d.NoRM,
                        d.NamaPasien,

                        d.DokTagihanPerawatan,
                        d.DokDetailBiaya,

                        d.TglTerimaDok,

                        d.Keterangan,

                        h.NoInvoice
                    };

                // SEARCH
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = $"%{search.Trim().ToLower()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.NoRM, search) ||
                        EF.Functions.ILike(x.NamaPasien, search) ||
                        EF.Functions.ILike(x.NoInvoice, search) ||
                        EF.Functions.ILike(x.Keterangan ?? "", search)
                    );
                }

                // FILTER DATE
                if (startDate.HasValue && endDate.HasValue)
                {
                    DateTime startUtc =
                        startDate.Value.Date.ToUniversalTime();

                    DateTime endUtc =
                        endDate.Value.Date
                        .AddDays(1)
                        .AddTicks(-1)
                        .ToUniversalTime();

                    query = query.Where(x =>
                        x.TglTerimaDok >= startUtc &&
                        x.TglTerimaDok <= endUtc);
                }

                // SORTING
                var sortColumn =
                    orderBy?.ToLower() ?? "tglterimadok";

                var isDescending =
                    sortDirection?.ToLower() == "desc";

                query = sortColumn switch
                {
                    "norm" =>
                        isDescending
                            ? query.OrderByDescending(x => x.NoRM)
                            : query.OrderBy(x => x.NoRM),

                    "namapasien" =>
                        isDescending
                            ? query.OrderByDescending(x => x.NamaPasien)
                            : query.OrderBy(x => x.NamaPasien),

                    "noinvoice" =>
                        isDescending
                            ? query.OrderByDescending(x => x.NoInvoice)
                            : query.OrderBy(x => x.NoInvoice),

                    "tglterimadok" =>
                        isDescending
                            ? query.OrderByDescending(x => x.TglTerimaDok)
                            : query.OrderBy(x => x.TglTerimaDok),

                    _ =>
                        query.OrderByDescending(x => x.TglTerimaDok)
                };

                // PAGINATION
                int totalRows = await query.CountAsync();

                int totalPages =
                    (int)Math.Ceiling(totalRows / (double)perPage);

                var rows = await query
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .ToListAsync();

                return Ok(new
                {
                    status = "success",
                    message = "Data berhasil diambil",

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
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                return StatusCode(500, new
                {
                    message = ex.Message
                });
            }
        }

        // =====================================================
        // GET BY ID
        // =====================================================

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var data = await _applicationDbContext.ARDokumens
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.ARDokumenId == id);

                if (data == null)
                {
                    return NotFound(new
                    {
                        message = "Data tidak ditemukan."
                    });
                }

                return Ok(new
                {
                    status = "success",
                    data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                return StatusCode(500, new
                {
                    message = ex.Message
                });
            }
        }

        // =====================================================
        // CREATE
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] ARDokumenViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                bool headerExists =
                    await _applicationDbContext.ARHeaders
                    .AnyAsync(x =>
                        x.ARHeaderId == vm.ARHeaderId &&
                        x.IsDelete == false);

                if (!headerExists)
                {
                    return NotFound(new
                    {
                        message = "AR Header tidak ditemukan."
                    });
                }

                var data = new ARDokumen
                {
                    ARDokumenId = Guid.NewGuid(),

                    ARHeaderId = vm.ARHeaderId,

                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,

                    NoRM = vm.NoRM,
                    NamaPasien = vm.NamaPasien,

                    DokTagihanPerawatan =
                        vm.DokTagihanPerawatan,

                    DokDetailBiaya =
                        vm.DokDetailBiaya,

                    TglTerimaDok =
                        vm.TglTerimaDok,

                    Keterangan = vm.Keterangan
                };

                _applicationDbContext.ARDokumens.Add(data);

                int result =
                    await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Created("", new
                    {
                        message = "Tambah data berhasil."
                    });
                }

                return StatusCode(500, new
                {
                    message = "Gagal menyimpan data."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                return StatusCode(500, new
                {
                    message = ex.Message
                });
            }
        }

        // =====================================================
        // UPDATE
        // =====================================================

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] ARDokumenViewModel vm)
        {
            try
            {
                var data =
                    await _applicationDbContext.ARDokumens
                    .FirstOrDefaultAsync(x =>
                        x.ARDokumenId == id);

                if (data == null)
                {
                    return NotFound(new
                    {
                        message = "Data tidak ditemukan."
                    });
                }

                data.ARHeaderId = vm.ARHeaderId;

                data.KunjunganId = vm.KunjunganId;
                data.PasienId = vm.PasienId;

                data.NoRM = vm.NoRM;
                data.NamaPasien = vm.NamaPasien;

                data.DokTagihanPerawatan =
                    vm.DokTagihanPerawatan;

                data.DokDetailBiaya =
                    vm.DokDetailBiaya;

                data.TglTerimaDok =
                    vm.TglTerimaDok;

                data.Keterangan =
                    vm.Keterangan;

                _applicationDbContext.ARDokumens.Update(data);

                int result =
                    await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new
                    {
                        message = "Update data berhasil."
                    });
                }

                return StatusCode(500, new
                {
                    message = "Gagal update data."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                return StatusCode(500, new
                {
                    message = ex.Message
                });
            }
        }

        // =====================================================
        // DELETE
        // =====================================================

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var data =
                    await _applicationDbContext.ARDokumens
                    .FirstOrDefaultAsync(x =>
                        x.ARDokumenId == id);

                if (data == null)
                {
                    return NotFound(new
                    {
                        message = "Data tidak ditemukan."
                    });
                }

                _applicationDbContext.ARDokumens.Remove(data);

                int result =
                    await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new
                    {
                        message = "Delete berhasil."
                    });
                }

                return StatusCode(500, new
                {
                    message = "Gagal delete data."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                return StatusCode(500, new
                {
                    message = ex.Message
                });
            }
        }
    }
}