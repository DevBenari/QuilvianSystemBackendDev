using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.Finance.Pembayaran.Models;
using QuilvianSystemBackendDev.Areas.Finance.Pembayaran.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using System.Data;
using System.Security.Claims;

namespace QuilvianSystemBackendDev.Areas.Finance.Pembayaran.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class CostCenterController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<CostCenterController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CostCenterController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<CostCenterController> logger,
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

            var userActive =
                await _applicationDbContext.UserActives
                .FirstOrDefaultAsync(x => x.Email == emailLogin);

            return userActive?.UserActiveId;
        }

        private async Task<string> GenerateKodeCostCenterAsync()
        {
            const string prefix = "CC-";

            var lastCode =
                await _applicationDbContext.CostCenters
                .AsNoTracking()
                .Where(x =>
                    x.IsDelete == false &&
                    x.KodeCostCenter != null &&
                    x.KodeCostCenter.StartsWith(prefix))
                .OrderByDescending(x => x.KodeCostCenter)
                .Select(x => x.KodeCostCenter)
                .FirstOrDefaultAsync();

            var nextNumber = 1001;

            if (!string.IsNullOrWhiteSpace(lastCode) && lastCode.Length > prefix.Length)
            {
                var numberPart =
                    lastCode.Substring(prefix.Length);

                if (int.TryParse(numberPart, out var lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber}";
        }

        [HttpGet("paged")]
        public async Task<IActionResult> PagedCostCenter(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "KodeCostCenter",
            string? sortDirection = "asc"
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

                if (perPage > 200)
                    perPage = 200;

                var query =
                    _applicationDbContext.CostCenters
                    .AsNoTracking()
                    .Where(x => x.IsDelete == false)
                    .Select(x => new
                    {
                        x.CostCenterId,
                        x.KodeCostCenter,
                        x.LokasiCostCenter,
                        x.Keterangan,
                        x.CreateDateTime,
                        x.UpdateDateTime
                    });

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var keyword = $"%{search.Trim()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.KodeCostCenter ?? "", keyword) ||
                        EF.Functions.ILike(x.LokasiCostCenter ?? "", keyword) ||
                        EF.Functions.ILike(x.Keterangan ?? "", keyword));
                }

                var sortColumn =
                    orderBy?.ToLower() ?? "kodecostcenter";

                var isDescending =
                    sortDirection?.ToLower() == "desc";

                query = sortColumn switch
                {
                    "kodecostcenter" =>
                        isDescending
                            ? query.OrderByDescending(x => x.KodeCostCenter)
                            : query.OrderBy(x => x.KodeCostCenter),

                    "lokasicostcenter" =>
                        isDescending
                            ? query.OrderByDescending(x => x.LokasiCostCenter)
                            : query.OrderBy(x => x.LokasiCostCenter),

                    "createdatetime" =>
                        isDescending
                            ? query.OrderByDescending(x => x.CreateDateTime)
                            : query.OrderBy(x => x.CreateDateTime),

                    _ =>
                        query.OrderBy(x => x.KodeCostCenter)
                };

                int totalRows =
                    await query.CountAsync();

                int totalPages =
                    (int)Math.Ceiling(totalRows / (double)perPage);

                if (totalRows == 0)
                {
                    return Ok(new
                    {
                        status = "success",
                        message = "No data found",
                        data = new
                        {
                            Rows = Array.Empty<object>(),
                            TotalRows = 0,
                            CurrentPage = page,
                            PerPage = perPage,
                            TotalPages = 0
                        }
                    });
                }

                if (page > totalPages)
                {
                    return NotFound(new
                    {
                        message = "Page not found."
                    });
                }

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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var data =
                    await _applicationDbContext.CostCenters
                    .AsNoTracking()
                    .Where(x =>
                        x.CostCenterId == id &&
                        x.IsDelete == false)
                    .Select(x => new
                    {
                        x.CostCenterId,
                        x.KodeCostCenter,
                        x.LokasiCostCenter,
                        x.Keterangan,
                        x.CreateDateTime,
                        x.UpdateDateTime
                    })
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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CostCenterViewModel vm)
        {
            using var transaction =
                await _applicationDbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userActiveId =
                    await GetUserActiveId();

                if (userActiveId == null)
                {
                    return Unauthorized(new
                    {
                        message = "User aktif tidak ditemukan."
                    });
                }

                var kodeCostCenter =
                    await GenerateKodeCostCenterAsync();

                var data = new CostCenter
                {
                    CostCenterId = Guid.NewGuid(),
                    KodeCostCenter = kodeCostCenter,
                    LokasiCostCenter = vm.LokasiCostCenter,
                    Keterangan = vm.Keterangan,

                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = userActiveId.Value,
                    IsDelete = false
                };

                _applicationDbContext.CostCenters.Add(data);

                int result =
                    await _applicationDbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                if (result > 0)
                {
                    return Created("", new
                    {
                        message = "Tambah data berhasil.",
                        data = new
                        {
                            data.CostCenterId,
                            data.KodeCostCenter
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
                await transaction.RollbackAsync();

                _logger.LogError(ex, ex.Message);

                return StatusCode(500, new
                {
                    message = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CostCenterViewModel vm)
        {
            using var transaction =
                await _applicationDbContext.Database.BeginTransactionAsync();

            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var data =
                    await _applicationDbContext.CostCenters
                    .FirstOrDefaultAsync(x =>
                        x.CostCenterId == id &&
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

                data.LokasiCostCenter = vm.LokasiCostCenter;
                data.Keterangan = vm.Keterangan;
                data.UpdateDateTime = DateTime.UtcNow;
                data.UpdateBy = userActiveId.Value;

                _applicationDbContext.CostCenters.Update(data);

                int result =
                    await _applicationDbContext.SaveChangesAsync();

                await transaction.CommitAsync();

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
                await transaction.RollbackAsync();

                _logger.LogError(ex, ex.Message);

                return StatusCode(500, new
                {
                    message = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            using var transaction =
                await _applicationDbContext.Database.BeginTransactionAsync();

            try
            {
                var data =
                    await _applicationDbContext.CostCenters
                    .FirstOrDefaultAsync(x =>
                        x.CostCenterId == id &&
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

                _applicationDbContext.CostCenters.Update(data);

                int result =
                    await _applicationDbContext.SaveChangesAsync();

                await transaction.CommitAsync();

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
                await transaction.RollbackAsync();

                _logger.LogError(ex, ex.Message);

                return StatusCode(500, new
                {
                    message = ex.Message
                });
            }
        }
    }
}