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
    public class DetailPembayaranManualController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<DetailPembayaranManualController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DetailPembayaranManualController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<DetailPembayaranManualController> logger,
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

        // =====================================================
        // PAGED DETAIL PEMBAYARAN MANUAL
        // =====================================================

        [HttpGet("paged")]
        public async Task<IActionResult> PagedDetailPembayaranManual(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            Guid? pembayaranManualId = null,
            Guid? coaId = null,
            Guid? costCenterId = null
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

                var baseQuery =
                    _applicationDbContext.DetailPembayaranManuals
                    .AsNoTracking()
                    .Where(x => x.IsDelete == false);

                if (pembayaranManualId.HasValue)
                {
                    baseQuery = baseQuery.Where(x =>
                        x.PembayaranManualId == pembayaranManualId.Value);
                }

                if (coaId.HasValue)
                {
                    baseQuery = baseQuery.Where(x =>
                        x.CoaId == coaId.Value);
                }

                if (costCenterId.HasValue)
                {
                    baseQuery = baseQuery.Where(x =>
                        x.CostCenterId == costCenterId.Value);
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var keyword = $"%{search.Trim()}%";

                    baseQuery = baseQuery.Where(x =>
                        EF.Functions.ILike(x.DeskripsiPembayaran ?? "", keyword) ||
                        EF.Functions.ILike(x.Keterangan ?? "", keyword) ||

                        _applicationDbContext.PembayaranManuals.Any(p =>
                            p.PembayaranManualId == x.PembayaranManualId &&
                            EF.Functions.ILike(p.KodePembayaranManual ?? "", keyword)
                        ) ||

                        _applicationDbContext.CostCenters.Any(cc =>
                            cc.CostCenterId == x.CostCenterId &&
                            (
                                EF.Functions.ILike(cc.KodeCostCenter ?? "", keyword) ||
                                EF.Functions.ILike(cc.LokasiCostCenter ?? "", keyword)
                            )
                        )
                    );
                }

                var query =
                    baseQuery.Select(x => new
                    {
                        x.DetailPembayaranManualId,
                        x.PembayaranManualId,

                        KodePembayaranManual =
                            _applicationDbContext.PembayaranManuals
                            .Where(p => p.PembayaranManualId == x.PembayaranManualId)
                            .Select(p => p.KodePembayaranManual)
                            .FirstOrDefault(),

                        x.CoaId,

                        NamaCoa =
                            _applicationDbContext.MasterCoas
                            .Where(c => c.COAId == x.CoaId)
                            .Select(c => c.NamaCOA)
                            .FirstOrDefault(),

                        x.DeskripsiPembayaran,
                        x.CostCenterId,

                        KodeCostCenter =
                            _applicationDbContext.CostCenters
                            .Where(cc => cc.CostCenterId == x.CostCenterId)
                            .Select(cc => cc.KodeCostCenter)
                            .FirstOrDefault(),

                        LokasiCostCenter =
                            _applicationDbContext.CostCenters
                            .Where(cc => cc.CostCenterId == x.CostCenterId)
                            .Select(cc => cc.LokasiCostCenter)
                            .FirstOrDefault(),

                        x.NominalPembayaran,
                        x.Keterangan,
                        x.CreateDateTime,
                        x.UpdateDateTime
                    });

                var sortColumn =
                    orderBy?.ToLower() ?? "createdatetime";

                var isDescending =
                    sortDirection?.ToLower() == "desc";

                query = sortColumn switch
                {
                    "kodepembayaranmanual" =>
                        isDescending
                            ? query.OrderByDescending(x => x.KodePembayaranManual)
                            : query.OrderBy(x => x.KodePembayaranManual),

                    "namacoa" =>
                        isDescending
                            ? query.OrderByDescending(x => x.NamaCoa)
                            : query.OrderBy(x => x.NamaCoa),

                    "deskripsipembayaran" =>
                        isDescending
                            ? query.OrderByDescending(x => x.DeskripsiPembayaran)
                            : query.OrderBy(x => x.DeskripsiPembayaran),

                    "kodecostcenter" =>
                        isDescending
                            ? query.OrderByDescending(x => x.KodeCostCenter)
                            : query.OrderBy(x => x.KodeCostCenter),

                    "nominalpembayaran" =>
                        isDescending
                            ? query.OrderByDescending(x => x.NominalPembayaran)
                            : query.OrderBy(x => x.NominalPembayaran),

                    "createdatetime" =>
                        isDescending
                            ? query.OrderByDescending(x => x.CreateDateTime)
                            : query.OrderBy(x => x.CreateDateTime),

                    _ =>
                        query.OrderByDescending(x => x.CreateDateTime)
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

        // =====================================================
        // GET DETAIL BY ID
        // =====================================================

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var data =
                    await _applicationDbContext.DetailPembayaranManuals
                    .AsNoTracking()
                    .Where(x =>
                        x.DetailPembayaranManualId == id &&
                        x.IsDelete == false)
                    .Select(x => new
                    {
                        x.DetailPembayaranManualId,
                        x.PembayaranManualId,

                        KodePembayaranManual =
                            _applicationDbContext.PembayaranManuals
                            .Where(p => p.PembayaranManualId == x.PembayaranManualId)
                            .Select(p => p.KodePembayaranManual)
                            .FirstOrDefault(),

                        x.CoaId,

                        NamaCoa =
                            _applicationDbContext.MasterCoas
                            .Where(c => c.COAId == x.CoaId)
                            .Select(c => c.NamaCOA)
                            .FirstOrDefault(),

                        x.DeskripsiPembayaran,
                        x.CostCenterId,

                        KodeCostCenter =
                            _applicationDbContext.CostCenters
                            .Where(cc => cc.CostCenterId == x.CostCenterId)
                            .Select(cc => cc.KodeCostCenter)
                            .FirstOrDefault(),

                        LokasiCostCenter =
                            _applicationDbContext.CostCenters
                            .Where(cc => cc.CostCenterId == x.CostCenterId)
                            .Select(cc => cc.LokasiCostCenter)
                            .FirstOrDefault(),

                        x.NominalPembayaran,
                        x.Keterangan
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

        // =====================================================
        // GET DETAIL BY PEMBAYARAN MANUAL ID
        // =====================================================

        [HttpGet("by-pembayaran-manual/{pembayaranManualId}")]
        public async Task<IActionResult> GetByPembayaranManualId(Guid pembayaranManualId)
        {
            try
            {
                var rows =
                    await _applicationDbContext.DetailPembayaranManuals
                    .AsNoTracking()
                    .Where(x =>
                        x.PembayaranManualId == pembayaranManualId &&
                        x.IsDelete == false)
                    .Select(x => new
                    {
                        x.DetailPembayaranManualId,
                        x.PembayaranManualId,
                        x.CoaId,

                        NamaCoa =
                            _applicationDbContext.MasterCoas
                            .Where(c => c.COAId == x.CoaId)
                            .Select(c => c.NamaCOA)
                            .FirstOrDefault(),

                        x.DeskripsiPembayaran,
                        x.CostCenterId,

                        KodeCostCenter =
                            _applicationDbContext.CostCenters
                            .Where(cc => cc.CostCenterId == x.CostCenterId)
                            .Select(cc => cc.KodeCostCenter)
                            .FirstOrDefault(),

                        LokasiCostCenter =
                            _applicationDbContext.CostCenters
                            .Where(cc => cc.CostCenterId == x.CostCenterId)
                            .Select(cc => cc.LokasiCostCenter)
                            .FirstOrDefault(),

                        x.NominalPembayaran,
                        x.Keterangan
                    })
                    .OrderByDescending(x => x.NominalPembayaran)
                    .ToListAsync();

                return Ok(new
                {
                    status = "success",
                    data = new
                    {
                        Rows = rows,
                        TotalRows = rows.Count,
                        TotalNominal = rows.Sum(x => x.NominalPembayaran)
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
        // CREATE DETAIL
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DetailPembayaranManualViewModel vm)
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

                var pembayaranManualExists =
                    await _applicationDbContext.PembayaranManuals
                    .AnyAsync(x =>
                        x.PembayaranManualId == vm.PembayaranManualId &&
                        x.IsDelete == false);

                if (!pembayaranManualExists)
                {
                    return BadRequest(new
                    {
                        message = "Pembayaran manual tidak ditemukan."
                    });
                }

                var costCenterExists =
                    await _applicationDbContext.CostCenters
                    .AnyAsync(x =>
                        x.CostCenterId == vm.CostCenterId &&
                        x.IsDelete == false);

                if (!costCenterExists)
                {
                    return BadRequest(new
                    {
                        message = "Cost center tidak ditemukan."
                    });
                }

                if (string.IsNullOrWhiteSpace(vm.DeskripsiPembayaran))
                {
                    return BadRequest(new
                    {
                        message = "Deskripsi pembayaran wajib diisi."
                    });
                }

                if (vm.NominalPembayaran <= 0)
                {
                    return BadRequest(new
                    {
                        message = "Nominal pembayaran harus lebih dari 0."
                    });
                }

                var data = new DetailPembayaranManual
                {
                    DetailPembayaranManualId = Guid.NewGuid(),
                    PembayaranManualId = vm.PembayaranManualId,
                    CoaId = vm.CoaId,
                    DeskripsiPembayaran = vm.DeskripsiPembayaran.Trim(),
                    CostCenterId = vm.CostCenterId,
                    NominalPembayaran = vm.NominalPembayaran,
                    Keterangan = vm.Keterangan,

                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = userActiveId.Value,
                    IsDelete = false
                };

                _applicationDbContext.DetailPembayaranManuals.Add(data);

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
                            data.DetailPembayaranManualId,
                            data.PembayaranManualId
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

        // =====================================================
        // UPDATE DETAIL
        // =====================================================

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] DetailPembayaranManualViewModel vm)
        {
            using var transaction =
                await _applicationDbContext.Database.BeginTransactionAsync();

            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var data =
                    await _applicationDbContext.DetailPembayaranManuals
                    .FirstOrDefaultAsync(x =>
                        x.DetailPembayaranManualId == id &&
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

                var pembayaranManualExists =
                    await _applicationDbContext.PembayaranManuals
                    .AnyAsync(x =>
                        x.PembayaranManualId == vm.PembayaranManualId &&
                        x.IsDelete == false);

                if (!pembayaranManualExists)
                {
                    return BadRequest(new
                    {
                        message = "Pembayaran manual tidak ditemukan."
                    });
                }

                var costCenterExists =
                    await _applicationDbContext.CostCenters
                    .AnyAsync(x =>
                        x.CostCenterId == vm.CostCenterId &&
                        x.IsDelete == false);

                if (!costCenterExists)
                {
                    return BadRequest(new
                    {
                        message = "Cost center tidak ditemukan."
                    });
                }

                if (string.IsNullOrWhiteSpace(vm.DeskripsiPembayaran))
                {
                    return BadRequest(new
                    {
                        message = "Deskripsi pembayaran wajib diisi."
                    });
                }

                if (vm.NominalPembayaran <= 0)
                {
                    return BadRequest(new
                    {
                        message = "Nominal pembayaran harus lebih dari 0."
                    });
                }

                data.PembayaranManualId = vm.PembayaranManualId;
                data.CoaId = vm.CoaId;
                data.DeskripsiPembayaran = vm.DeskripsiPembayaran.Trim();
                data.CostCenterId = vm.CostCenterId;
                data.NominalPembayaran = vm.NominalPembayaran;
                data.Keterangan = vm.Keterangan;

                data.UpdateDateTime = DateTime.UtcNow;
                data.UpdateBy = userActiveId.Value;

                _applicationDbContext.DetailPembayaranManuals.Update(data);

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

        // =====================================================
        // DELETE DETAIL
        // =====================================================

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            using var transaction =
                await _applicationDbContext.Database.BeginTransactionAsync();

            try
            {
                var data =
                    await _applicationDbContext.DetailPembayaranManuals
                    .FirstOrDefaultAsync(x =>
                        x.DetailPembayaranManualId == id &&
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

                _applicationDbContext.DetailPembayaranManuals.Update(data);

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