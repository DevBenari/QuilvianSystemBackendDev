using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.Finance.All.Models;
using QuilvianSystemBackendDev.Repositories;
using System.Security.Claims;

namespace QuilvianSystemBackendDev.Areas.Finance.All.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class RecurringJournalDetailController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RecurringJournalDetailController> _logger;

        public RecurringJournalDetailController(
            ApplicationDbContext context,
            ILogger<RecurringJournalDetailController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("paged")]
        public async Task<IActionResult> Paged(
            int page = 1,
            int perPage = 10,
            Guid? tempRJId = null,
            string? search = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc")
        {
            try
            {
                page = page < 1 ? 1 : page;
                perPage = perPage < 1 ? 10 : perPage;

                var query = _context.RecurringJournalDetails
                    .AsNoTracking()
                    .Where(x =>
                        x.IsDelete == false ||
                        x.IsDelete == null);

                if (tempRJId.HasValue)
                {
                    query = query.Where(x =>
                        x.TempRJId == tempRJId.Value);
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var keyword = $"%{search.Trim()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(
                            x.COACode ?? "",
                            keyword) ||
                        EF.Functions.ILike(
                            x.COAName ?? "",
                            keyword) ||
                        EF.Functions.ILike(
                            x.RoleSetupCOA ?? "",
                            keyword) ||
                        EF.Functions.ILike(
                            x.NoRegistrasi ?? "",
                            keyword) ||
                        EF.Functions.ILike(
                            x.CostCenterName ?? "",
                            keyword) ||
                        EF.Functions.ILike(
                            x.Keterangan ?? "",
                            keyword));
                }

                var isDescending = string.Equals(
                    sortDirection,
                    "desc",
                    StringComparison.OrdinalIgnoreCase);

                query = orderBy?.Trim().ToLower() switch
                {
                    "coacode" =>
                        isDescending
                            ? query.OrderByDescending(x =>
                                x.COACode)
                            : query.OrderBy(x =>
                                x.COACode),

                    "coaname" =>
                        isDescending
                            ? query.OrderByDescending(x =>
                                x.COAName)
                            : query.OrderBy(x =>
                                x.COAName),

                    "debetamount" =>
                        isDescending
                            ? query.OrderByDescending(x =>
                                x.DebetAmount)
                            : query.OrderBy(x =>
                                x.DebetAmount),

                    "creditamount" =>
                        isDescending
                            ? query.OrderByDescending(x =>
                                x.CreditAmount)
                            : query.OrderBy(x =>
                                x.CreditAmount),

                    _ =>
                        isDescending
                            ? query.OrderByDescending(x =>
                                x.CreateDateTime)
                            : query.OrderBy(x =>
                                x.CreateDateTime)
                };

                var totalRows = await query.CountAsync();

                var rows = await query
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .ToListAsync();

                return Ok(new
                {
                    status = "success",
                    message = "Data berhasil diambil.",
                    data = new
                    {
                        Rows = rows,
                        TotalRows = totalRows,
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalPages = totalRows == 0
                            ? 0
                            : (int)Math.Ceiling(
                                totalRows / (double)perPage)
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

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var data = await _context
                    .RecurringJournalDetails
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.DetailTempRJId == id &&
                        (x.IsDelete == false ||
                         x.IsDelete == null));

                if (data == null)
                {
                    return NotFound(new
                    {
                        message =
                            "Detail recurring journal tidak ditemukan."
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
        public async Task<IActionResult> Create(
            [FromBody] RecurringJournalDetail request)
        {
            try
            {
                var userActiveId =
                    await GetCurrentUserActiveId();

                if (!userActiveId.HasValue)
                {
                    return Unauthorized(new
                    {
                        message =
                            "User aktif tidak ditemukan."
                    });
                }

                var headerExists = await _context
                    .RecurringJournals
                    .AnyAsync(x =>
                        x.TempRJId == request.TempRJId &&
                        (x.IsDelete == false ||
                         x.IsDelete == null));

                if (!headerExists)
                {
                    return BadRequest(new
                    {
                        message =
                            "Recurring journal tidak ditemukan."
                    });
                }

                var entity = new RecurringJournalDetail
                {
                    DetailTempRJId = Guid.NewGuid(),
                    TempRJId = request.TempRJId,

                    COAId = request.COAId,
                    COACode = request.COACode?.Trim(),
                    COAName = request.COAName?.Trim(),
                    RoleSetupCOA =
                        request.RoleSetupCOA?.Trim(),

                    DebetAmount = request.DebetAmount,
                    CreditAmount = request.CreditAmount,

                    KunjunganId = request.KunjunganId,
                    NoRegistrasi =
                        request.NoRegistrasi?.Trim(),

                    CostCenterId = request.CostCenterId,
                    CostCenterName =
                        request.CostCenterName?.Trim(),

                    Keterangan =
                        request.Keterangan?.Trim(),

                    IsDelete = false,
                    CreateBy = userActiveId.Value,
                    CreateDateTime = DateTime.UtcNow
                };

                _context.RecurringJournalDetails.Add(entity);
                await _context.SaveChangesAsync();

                return Created("", new
                {
                    status = "success",
                    message =
                        "Detail recurring journal berhasil ditambahkan.",
                    data = entity
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

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] RecurringJournalDetail request)
        {
            try
            {
                var userActiveId =
                    await GetCurrentUserActiveId();

                if (!userActiveId.HasValue)
                {
                    return Unauthorized(new
                    {
                        message =
                            "User aktif tidak ditemukan."
                    });
                }

                var entity = await _context
                    .RecurringJournalDetails
                    .FirstOrDefaultAsync(x =>
                        x.DetailTempRJId == id &&
                        (x.IsDelete == false ||
                         x.IsDelete == null));

                if (entity == null)
                {
                    return NotFound(new
                    {
                        message =
                            "Detail recurring journal tidak ditemukan."
                    });
                }

                entity.TempRJId = request.TempRJId;

                entity.COAId = request.COAId;
                entity.COACode = request.COACode?.Trim();
                entity.COAName = request.COAName?.Trim();
                entity.RoleSetupCOA =
                    request.RoleSetupCOA?.Trim();

                entity.DebetAmount = request.DebetAmount;
                entity.CreditAmount = request.CreditAmount;

                entity.KunjunganId = request.KunjunganId;
                entity.NoRegistrasi =
                    request.NoRegistrasi?.Trim();

                entity.CostCenterId = request.CostCenterId;
                entity.CostCenterName =
                    request.CostCenterName?.Trim();

                entity.Keterangan =
                    request.Keterangan?.Trim();

                entity.UpdateBy = userActiveId.Value;
                entity.UpdateDateTime = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    status = "success",
                    message =
                        "Detail recurring journal berhasil diperbarui."
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

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var userActiveId =
                    await GetCurrentUserActiveId();

                if (!userActiveId.HasValue)
                {
                    return Unauthorized(new
                    {
                        message =
                            "User aktif tidak ditemukan."
                    });
                }

                var entity = await _context
                    .RecurringJournalDetails
                    .FirstOrDefaultAsync(x =>
                        x.DetailTempRJId == id &&
                        (x.IsDelete == false ||
                         x.IsDelete == null));

                if (entity == null)
                {
                    return NotFound(new
                    {
                        message =
                            "Detail recurring journal tidak ditemukan."
                    });
                }

                entity.IsDelete = true;
                entity.DeleteBy = userActiveId.Value;
                entity.DeleteDateTime = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    status = "success",
                    message =
                        "Detail recurring journal berhasil dihapus."
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

        private async Task<Guid?> GetCurrentUserActiveId()
        {
            var email = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(email))
                return null;

            return await _context.UserActives
                .Where(x => x.Email == email)
                .Select(x => (Guid?)x.UserActiveId)
                .FirstOrDefaultAsync();
        }
    }
}
