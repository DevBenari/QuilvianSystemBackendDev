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
    public class ARHeaderController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<ARHeaderController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ARHeaderController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ARHeaderController> logger,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _applicationDbContext = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }
        // =========================================================
        // GET ALL
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> GetAll()
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

                var data =
                    await (
                        from ar in _applicationDbContext.ARHeaders.AsNoTracking()

                        join u in _applicationDbContext.UserActives.AsNoTracking()
                        on ar.CreateBy equals u.UserActiveId

                        where ar.IsDelete == false

                        orderby ar.CreateDateTime descending

                        select new
                        {
                            ar.ARHeaderId,
                            ar.AsuransiId,

                            ar.NoInvoice,

                            ar.TglPembuatanInvoice,
                            ar.TglKirim,
                            ar.TglTerima,
                            ar.TglTagihan,
                            ar.TglJatuhTempo,

                            ar.DueDate,
                            ar.TotalInvoice,

                            ar.IsDocumentComplited,

                            ar.Keterangan,

                            ar.CreateDateTime,
                            ar.CreateBy,

                            CreateByName = u.FullName
                        }
                    ).ToListAsync();

                return Ok(new
                {
                    status = "success",
                    message = "Data berhasil diambil",
                    data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                return StatusCode(500, new
                {
                    message = $"Terjadi kesalahan internal : {ex.Message}"
                });
            }
        }
        // =========================================================
        // GET ALL PAGED
        // =========================================================

        [HttpGet("paged")]
        public async Task<IActionResult> PagedARHeader(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",

            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
            DateTime? startDate = null,

            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
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
                    from ar in _applicationDbContext.ARHeaders.AsNoTracking()

                    join u in _applicationDbContext.UserActives.AsNoTracking()
                    on ar.CreateBy equals u.UserActiveId

                    where ar.IsDelete == false

                    select new
                    {
                        ar.ARHeaderId,
                        ar.AsuransiId,

                        ar.NoInvoice,

                        ar.TglPembuatanInvoice,
                        ar.TglKirim,
                        ar.TglTerima,
                        ar.TglTagihan,
                        ar.TglJatuhTempo,

                        ar.DueDate,
                        ar.TotalInvoice,

                        ar.IsDocumentComplited,

                        ar.Keterangan,

                        ar.CreateDateTime,
                        ar.CreateBy,

                        CreateByName = u.FullName
                    };

                // SEARCH
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = $"%{search.Trim().ToLower()}%";

                    query = query.Where(x =>
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
                        x.CreateDateTime >= startUtc &&
                        x.CreateDateTime <= endUtc);
                }

                // SORTING
                var sortColumn = orderBy?.ToLower() ?? "createdatetime";

                var isDescending =
                    sortDirection?.ToLower() == "desc";

                query = sortColumn switch
                {
                    "noinvoice" =>
                        isDescending
                            ? query.OrderByDescending(x => x.NoInvoice)
                            : query.OrderBy(x => x.NoInvoice),

                    "totalinvoice" =>
                        isDescending
                            ? query.OrderByDescending(x => x.TotalInvoice)
                            : query.OrderBy(x => x.TotalInvoice),

                    "createbyname" =>
                        isDescending
                            ? query.OrderByDescending(x => x.CreateByName)
                            : query.OrderBy(x => x.CreateByName),

                    "createdatetime" =>
                        isDescending
                            ? query.OrderByDescending(x => x.CreateDateTime)
                            : query.OrderBy(x => x.CreateDateTime),

                    _ =>
                        query.OrderByDescending(x => x.CreateDateTime)
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
                    message = $"Terjadi kesalahan internal : {ex.Message}"
                });
            }
        }

        // =========================================================
        // GET BY ID
        // =========================================================

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var data = await _applicationDbContext.ARHeaders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.ARHeaderId == id &&
                        x.IsDelete == false);

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

        // =========================================================
        // CREATE
        // =========================================================

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] ARHeaderViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var emailLogin =
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new
                    {
                        message = "User tidak terautentikasi."
                    });
                }

                var getUserActive =
                    await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(x =>
                        x.Email == emailLogin);

                if (getUserActive == null)
                {
                    return Unauthorized(new
                    {
                        message = "User aktif tidak ditemukan."
                    });
                }

                bool isDuplicate =
                    await _applicationDbContext.ARHeaders
                    .AnyAsync(x =>
                        x.NoInvoice.ToLower() ==
                        vm.NoInvoice.ToLower()
                        &&
                        x.IsDelete == false);

                if (isDuplicate)
                {
                    return Conflict(new
                    {
                        message = "No Invoice sudah digunakan."
                    });
                }

                var data = new ARHeader
                {
                    ARHeaderId = Guid.NewGuid(),

                    AsuransiId = vm.AsuransiId,

                    NoInvoice = vm.NoInvoice.Trim(),

                    TglPembuatanInvoice = vm.TglPembuatanInvoice,

                    DueDate = vm.DueDate,

                    TotalInvoice = vm.TotalInvoice,

                    TglKirim = vm.TglKirim,
                    TglTerima = vm.TglTerima,
                    TglTagihan = vm.TglTagihan,
                    TglJatuhTempo = vm.TglJatuhTempo,

                    IsDocumentComplited = vm.IsDocumentComplited,

                    Keterangan = vm.Keterangan,

                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = getUserActive.UserActiveId,

                    IsDelete = false
                };

                _applicationDbContext.ARHeaders.Add(data);

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

        // =========================================================
        // UPDATE
        // =========================================================

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] ARHeaderViewModel vm)
        {
            try
            {
                var data =
                    await _applicationDbContext.ARHeaders
                    .FirstOrDefaultAsync(x =>
                        x.ARHeaderId == id &&
                        x.IsDelete == false);

                if (data == null)
                {
                    return NotFound(new
                    {
                        message = "Data tidak ditemukan."
                    });
                }

                var emailLogin =
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var getUserActive =
                    await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(x =>
                        x.Email == emailLogin);

                if (getUserActive == null)
                {
                    return Unauthorized(new
                    {
                        message = "User aktif tidak ditemukan."
                    });
                }

                bool isDuplicate =
                    await _applicationDbContext.ARHeaders
                    .AnyAsync(x =>
                        x.NoInvoice.ToLower() ==
                        vm.NoInvoice.ToLower()
                        &&
                        x.ARHeaderId != id
                        &&
                        x.IsDelete == false);

                if (isDuplicate)
                {
                    return Conflict(new
                    {
                        message = "No Invoice sudah digunakan."
                    });
                }

                data.AsuransiId = vm.AsuransiId;

                data.NoInvoice = vm.NoInvoice.Trim();

                data.TglPembuatanInvoice =
                    vm.TglPembuatanInvoice;

                data.DueDate = vm.DueDate;

                data.TotalInvoice = vm.TotalInvoice;

                data.TglKirim = vm.TglKirim;
                data.TglTerima = vm.TglTerima;
                data.TglTagihan = vm.TglTagihan;
                data.TglJatuhTempo = vm.TglJatuhTempo;

                data.IsDocumentComplited =
                    vm.IsDocumentComplited;

                data.Keterangan = vm.Keterangan;

                data.UpdateDateTime = DateTime.UtcNow;
                data.UpdateBy = getUserActive.UserActiveId;

                _applicationDbContext.ARHeaders.Update(data);

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

        // =========================================================
        // DELETE
        // =========================================================

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var data =
                    await _applicationDbContext.ARHeaders
                    .FirstOrDefaultAsync(x =>
                        x.ARHeaderId == id &&
                        x.IsDelete == false);

                if (data == null)
                {
                    return NotFound(new
                    {
                        message = "Data tidak ditemukan."
                    });
                }

                var emailLogin =
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var getUserActive =
                    await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(x =>
                        x.Email == emailLogin);

                if (getUserActive == null)
                {
                    return Unauthorized(new
                    {
                        message = "User aktif tidak ditemukan."
                    });
                }

                data.IsDelete = true;

                data.DeleteDateTime = DateTime.UtcNow;
                data.DeleteBy = getUserActive.UserActiveId;

                _applicationDbContext.ARHeaders.Update(data);

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