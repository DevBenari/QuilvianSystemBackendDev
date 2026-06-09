using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.Finance.Faktur.Models;
using QuilvianSystemBackendDev.Areas.Finance.Faktur.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace QuilvianSystemBackendDev.Areas.Finance.Faktur.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class DetailTukarFakturController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<DetailTukarFakturController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DetailTukarFakturController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<DetailTukarFakturController> logger,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _applicationDbContext = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        private async Task<Guid?> GetUserActiveId()
        {
            var emailLogin =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(emailLogin))
                return null;

            var getUserActive =
                await _applicationDbContext.UserActives
                .FirstOrDefaultAsync(x =>
                    x.Email == emailLogin);

            if (getUserActive == null)
                return null;

            return getUserActive.UserActiveId;
        }

        // =====================================================
        // PAGED
        // =====================================================

        [HttpGet("paged")]
        public async Task<IActionResult> PagedDetailTukarFaktur(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "NoInvoice",
            string? sortDirection = "asc",
            Guid? tukarFakturId = null,
            Guid? supplierId = null,

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
                    from d in _applicationDbContext.DetailTukarFakturs
                        .AsNoTracking()

                    join h in _applicationDbContext.TukarFakturs
                        .AsNoTracking()
                    on d.TukarFakturId equals h.TukarFakturId

                    where d.IsDelete == false &&
                          h.IsDelete == false

                    select new
                    {
                        d.DetailTukarFakturId,
                        d.TukarFakturId,

                        d.NomorPO,
                        d.NoInvoice,
                        d.TotalInvoice,
                        d.Keterangan,

                        h.SupplierId,
                        h.TglRegistrasi,
                        h.TglTerimaFaktur,
                        h.TglJatuhTempo
                    };

                // SEARCH
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = $"%{search.Trim().ToLower()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.NomorPO ?? "", search) ||
                        EF.Functions.ILike(x.NoInvoice ?? "", search) ||
                        EF.Functions.ILike(x.Keterangan ?? "", search)
                    );
                }

                // FILTER TUKAR FAKTUR ID
                if (tukarFakturId.HasValue)
                {
                    query = query.Where(x =>
                        x.TukarFakturId == tukarFakturId.Value);
                }

                // FILTER SUPPLIER ID
                if (supplierId.HasValue)
                {
                    query = query.Where(x =>
                        x.SupplierId == supplierId.Value);
                }

                // FILTER DATE BERDASARKAN TGL REGISTRASI HEADER
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
                        x.TglRegistrasi >= startUtc &&
                        x.TglRegistrasi <= endUtc);
                }

                // SORTING
                var sortColumn =
                    orderBy?.ToLower() ?? "noinvoice";

                var isDescending =
                    sortDirection?.ToLower() == "desc";

                query = sortColumn switch
                {
                    "nomorpo" =>
                        isDescending
                            ? query.OrderByDescending(x => x.NomorPO)
                            : query.OrderBy(x => x.NomorPO),

                    "noinvoice" =>
                        isDescending
                            ? query.OrderByDescending(x => x.NoInvoice)
                            : query.OrderBy(x => x.NoInvoice),

                    "totalinvoice" =>
                        isDescending
                            ? query.OrderByDescending(x => x.TotalInvoice)
                            : query.OrderBy(x => x.TotalInvoice),

                    "tglregistrasi" =>
                        isDescending
                            ? query.OrderByDescending(x => x.TglRegistrasi)
                            : query.OrderBy(x => x.TglRegistrasi),

                    "tglterimafaktur" =>
                        isDescending
                            ? query.OrderByDescending(x => x.TglTerimaFaktur)
                            : query.OrderBy(x => x.TglTerimaFaktur),

                    "tgljatuhtempo" =>
                        isDescending
                            ? query.OrderByDescending(x => x.TglJatuhTempo)
                            : query.OrderBy(x => x.TglJatuhTempo),

                    _ =>
                        query.OrderBy(x => x.NoInvoice)
                };

                // PAGINATION
                int totalRows =
                    await query.CountAsync();

                int totalPages =
                    (int)Math.Ceiling(totalRows / (double)perPage);

                var rows =
                    await query
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
                var data =
                    await
                    (
                        from d in _applicationDbContext.DetailTukarFakturs
                            .AsNoTracking()

                        join h in _applicationDbContext.TukarFakturs
                            .AsNoTracking()
                        on d.TukarFakturId equals h.TukarFakturId

                        where d.DetailTukarFakturId == id &&
                              d.IsDelete == false &&
                              h.IsDelete == false

                        select new
                        {
                            d.DetailTukarFakturId,
                            d.TukarFakturId,

                            d.NomorPO,
                            d.NoInvoice,
                            d.TotalInvoice,
                            d.Keterangan,

                            h.SupplierId,
                            h.TglRegistrasi,
                            h.TglTerimaFaktur,
                            h.TglJatuhTempo
                        }
                    )
                    .FirstOrDefaultAsync();

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
            [FromBody] DetailTukarFakturViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                bool headerExists =
                    await _applicationDbContext.TukarFakturs
                    .AnyAsync(x =>
                        x.TukarFakturId == vm.TukarFakturId &&
                        x.IsDelete == false);

                if (!headerExists)
                {
                    return NotFound(new
                    {
                        message = "Tukar Faktur tidak ditemukan."
                    });
                }

                var userActiveId =
                    await GetUserActiveId();

                if (userActiveId == null)
                {
                    return Unauthorized(new
                    {
                        message = "User aktif tidak ditemukan."
                    });
                }

                var data = new DetailTukarFaktur
                {
                    DetailTukarFakturId = Guid.NewGuid(),
                    TukarFakturId = vm.TukarFakturId,

                    NomorPO = vm.NomorPO,
                    NoInvoice = vm.NoInvoice,
                    TotalInvoice = vm.TotalInvoice,
                    Keterangan = vm.Keterangan,

                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = userActiveId.Value,
                    IsDelete = false
                };

                _applicationDbContext.DetailTukarFakturs.Add(data);

                int result =
                    await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Created("", new
                    {
                        message = "Tambah data berhasil.",
                        data = new
                        {
                            data.DetailTukarFakturId
                        }
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
            [FromBody] DetailTukarFakturViewModel vm)
        {
            try
            {
                var data =
                    await _applicationDbContext.DetailTukarFakturs
                    .FirstOrDefaultAsync(x =>
                        x.DetailTukarFakturId == id &&
                        x.IsDelete == false);

                if (data == null)
                {
                    return NotFound(new
                    {
                        message = "Data tidak ditemukan."
                    });
                }

                bool headerExists =
                    await _applicationDbContext.TukarFakturs
                    .AnyAsync(x =>
                        x.TukarFakturId == vm.TukarFakturId &&
                        x.IsDelete == false);

                if (!headerExists)
                {
                    return NotFound(new
                    {
                        message = "Tukar Faktur tidak ditemukan."
                    });
                }

                var userActiveId =
                    await GetUserActiveId();

                if (userActiveId == null)
                {
                    return Unauthorized(new
                    {
                        message = "User aktif tidak ditemukan."
                    });
                }

                data.TukarFakturId = vm.TukarFakturId;

                data.NomorPO = vm.NomorPO;
                data.NoInvoice = vm.NoInvoice;
                data.TotalInvoice = vm.TotalInvoice;
                data.Keterangan = vm.Keterangan;

                data.UpdateDateTime = DateTime.UtcNow;
                data.UpdateBy = userActiveId.Value;

                _applicationDbContext.DetailTukarFakturs.Update(data);

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
                    await _applicationDbContext.DetailTukarFakturs
                    .FirstOrDefaultAsync(x =>
                        x.DetailTukarFakturId == id &&
                        x.IsDelete == false);

                if (data == null)
                {
                    return NotFound(new
                    {
                        message = "Data tidak ditemukan."
                    });
                }

                var userActiveId =
                    await GetUserActiveId();

                if (userActiveId == null)
                {
                    return Unauthorized(new
                    {
                        message = "User aktif tidak ditemukan."
                    });
                }

                data.IsDelete = true;
                data.DeleteDateTime = DateTime.UtcNow;
                data.DeleteBy = userActiveId.Value;

                _applicationDbContext.DetailTukarFakturs.Update(data);

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
