
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
    public class RecurringJournalController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RecurringJournalController> _logger;

        public RecurringJournalController(
            ApplicationDbContext context,
            ILogger<RecurringJournalController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("paged")]
        public async Task<IActionResult> Paged(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc")
        {
            try
            {
                page = page < 1 ? 1 : page;
                perPage = perPage < 1 ? 10 : perPage;

                var query = _context.RecurringJournals
                    .AsNoTracking()
                    .Where(x =>
                        x.IsDelete == false ||
                        x.IsDelete == null);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var keyword = $"%{search.Trim()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(
                            x.RecurringJournalName ?? "",
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
                    "recurringjournalname" =>
                        isDescending
                            ? query.OrderByDescending(x =>
                                x.RecurringJournalName)
                            : query.OrderBy(x =>
                                x.RecurringJournalName),

                    "recurringjournaldate" =>
                        isDescending
                            ? query.OrderByDescending(x =>
                                x.RecurringJournalDate)
                            : query.OrderBy(x =>
                                x.RecurringJournalDate),

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
                    .Select(x => new
                    {
                        x.TempRJId,
                        x.RecurringJournalName,
                        x.RecurringJournalDate,
                        x.Keterangan,
                        x.CreateDateTime,
                        x.CreateBy,
                        x.UpdateDateTime,
                        x.UpdateBy
                    })
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
                var data = await _context.RecurringJournals
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.TempRJId == id &&
                        (x.IsDelete == false ||
                         x.IsDelete == null));

                if (data == null)
                {
                    return NotFound(new
                    {
                        message =
                            "Recurring journal tidak ditemukan."
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
            [FromBody] RecurringJournal request)
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

                if (string.IsNullOrWhiteSpace(
                    request.RecurringJournalName))
                {
                    return BadRequest(new
                    {
                        message =
                            "Recurring journal wajib diisi."
                    });
                }

                var entity = new RecurringJournal
                {
                    TempRJId = Guid.NewGuid(),
                    RecurringJournalName =
                        request.RecurringJournalName.Trim(),
                    RecurringJournalDate =
                        request.RecurringJournalDate,
                    Keterangan =
                        request.Keterangan?.Trim(),
                    IsDelete = false,
                    CreateBy = userActiveId.Value,
                    CreateDateTime = DateTime.UtcNow
                };

                _context.RecurringJournals.Add(entity);
                await _context.SaveChangesAsync();

                return Created("", new
                {
                    status = "success",
                    message =
                        "Recurring journal berhasil ditambahkan.",
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
            [FromBody] RecurringJournal request)
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

                var entity = await _context.RecurringJournals
                    .FirstOrDefaultAsync(x =>
                        x.TempRJId == id &&
                        (x.IsDelete == false ||
                         x.IsDelete == null));

                if (entity == null)
                {
                    return NotFound(new
                    {
                        message =
                            "Recurring journal tidak ditemukan."
                    });
                }

                entity.RecurringJournalName =
                    request.RecurringJournalName.Trim();

                entity.RecurringJournalDate =
                    request.RecurringJournalDate;

                entity.Keterangan =
                    request.Keterangan?.Trim();

                entity.UpdateBy = userActiveId.Value;
                entity.UpdateDateTime = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    status = "success",
                    message =
                        "Recurring journal berhasil diperbarui."
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

                var entity = await _context.RecurringJournals
                    .FirstOrDefaultAsync(x =>
                        x.TempRJId == id &&
                        (x.IsDelete == false ||
                         x.IsDelete == null));

                if (entity == null)
                {
                    return NotFound(new
                    {
                        message =
                            "Recurring journal tidak ditemukan."
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
                        "Recurring journal berhasil dihapus."
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