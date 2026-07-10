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
    public class AccManualJurnalController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccManualJurnalController> _logger;

        public AccManualJurnalController(
            ApplicationDbContext context,
            ILogger<AccManualJurnalController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("paged")]
        public async Task<IActionResult> Paged(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? tipeDokumen = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc")
        {
            try
            {
                page = page < 1 ? 1 : page;
                perPage = perPage < 1 ? 10 : perPage;

                var query = _context.AccManualJurnals
                    .AsNoTracking()
                    .Where(x =>
                        x.IsDelete == false ||
                        x.IsDelete == null);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var keyword = $"%{search.Trim()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(
                            x.KodeManualJurnal ?? "",
                            keyword) ||
                        EF.Functions.ILike(
                            x.TipeDokumen ?? "",
                            keyword) ||
                        EF.Functions.ILike(
                            x.RecurringJournalName ?? "",
                            keyword) ||
                        EF.Functions.ILike(
                            x.NamaMataUang ?? "",
                            keyword) ||
                        EF.Functions.ILike(
                            x.Keterangan ?? "",
                            keyword));
                }

                if (!string.IsNullOrWhiteSpace(tipeDokumen))
                {
                    query = query.Where(x =>
                        x.TipeDokumen == tipeDokumen);
                }

                if (startDate.HasValue)
                {
                    query = query.Where(x =>
                        x.TglDokumen >= startDate.Value.Date);
                }

                if (endDate.HasValue)
                {
                    var endDateExclusive =
                        endDate.Value.Date.AddDays(1);

                    query = query.Where(x =>
                        x.TglDokumen < endDateExclusive);
                }

                var isDescending = string.Equals(
                    sortDirection,
                    "desc",
                    StringComparison.OrdinalIgnoreCase);

                query = orderBy?.Trim().ToLower() switch
                {
                    "kodemanualjurnal" =>
                        isDescending
                            ? query.OrderByDescending(x =>
                                x.KodeManualJurnal)
                            : query.OrderBy(x =>
                                x.KodeManualJurnal),

                    "tgldokumen" =>
                        isDescending
                            ? query.OrderByDescending(x =>
                                x.TglDokumen)
                            : query.OrderBy(x =>
                                x.TglDokumen),

                    "tglmanualjurnal" =>
                        isDescending
                            ? query.OrderByDescending(x =>
                                x.TglManualJurnal)
                            : query.OrderBy(x =>
                                x.TglManualJurnal),

                    "tipedokumen" =>
                        isDescending
                            ? query.OrderByDescending(x =>
                                x.TipeDokumen)
                            : query.OrderBy(x =>
                                x.TipeDokumen),

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
                        x.AccManualJurnalId,
                        x.KodeManualJurnal,
                        x.TglDokumen,
                        x.TglManualJurnal,
                        x.TglPembatalan,
                        x.TipeDokumen,

                        x.TempRJId,
                        x.RecurringJournalName,
                        x.RecurringJournalDate,

                        x.MataUangId,
                        x.NamaMataUang,

                        x.ExchangeRateId,
                        x.RateToIdr,
                        x.UnbalancedAmount,

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
                var data = await _context.AccManualJurnals
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.AccManualJurnalId == id &&
                        (x.IsDelete == false ||
                         x.IsDelete == null));

                if (data == null)
                {
                    return NotFound(new
                    {
                        message =
                            "Jurnal manual tidak ditemukan."
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
            [FromBody] AccManualJurnal request)
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

                var kodeManualJurnal =
                    await GenerateKodeManualJurnal();

                RecurringJournal? recurringJournal = null;

                if (request.TempRJId.HasValue)
                {
                    recurringJournal = await _context
                        .RecurringJournals
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.TempRJId ==
                            request.TempRJId.Value &&
                            (x.IsDelete == false ||
                             x.IsDelete == null));

                    if (recurringJournal == null)
                    {
                        return BadRequest(new
                        {
                            message =
                                "Recurring journal tidak ditemukan."
                        });
                    }
                }

                var entity = new AccManualJurnal
                {
                    AccManualJurnalId = Guid.NewGuid(),

                    KodeManualJurnal =
                        kodeManualJurnal,

                    TglDokumen =
                        request.TglDokumen,

                    TglManualJurnal =
                        request.TglManualJurnal,

                    TglPembatalan =
                        request.TglPembatalan,

                    TipeDokumen =
                        request.TipeDokumen?.Trim(),

                    TempRJId =
                        request.TempRJId,

                    RecurringJournalName =
                        recurringJournal?
                            .RecurringJournalName,

                    RecurringJournalDate =
                        recurringJournal?
                            .RecurringJournalDate,

                    MataUangId =
                        request.MataUangId,

                    NamaMataUang =
                        request.NamaMataUang?.Trim(),

                    ExchangeRateId =
                        request.ExchangeRateId,

                    RateToIdr =
                        request.RateToIdr,

                    UnbalancedAmount =
                        request.UnbalancedAmount,

                    Keterangan =
                        request.Keterangan?.Trim(),

                    IsDelete = false,
                    CreateBy = userActiveId.Value,
                    CreateDateTime = DateTime.UtcNow
                };

                _context.AccManualJurnals.Add(entity);

                await _context.SaveChangesAsync();

                return Created("", new
                {
                    status = "success",
                    message =
                        "Jurnal manual berhasil ditambahkan.",
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
            [FromBody] AccManualJurnal request)
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

                var entity = await _context.AccManualJurnals
                    .FirstOrDefaultAsync(x =>
                        x.AccManualJurnalId == id &&
                        (x.IsDelete == false ||
                         x.IsDelete == null));

                if (entity == null)
                {
                    return NotFound(new
                    {
                        message =
                            "Jurnal manual tidak ditemukan."
                    });
                }

                RecurringJournal? recurringJournal = null;

                if (request.TempRJId.HasValue)
                {
                    recurringJournal = await _context
                        .RecurringJournals
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.TempRJId ==
                            request.TempRJId.Value &&
                            (x.IsDelete == false ||
                             x.IsDelete == null));

                    if (recurringJournal == null)
                    {
                        return BadRequest(new
                        {
                            message =
                                "Recurring journal tidak ditemukan."
                        });
                    }
                }

                entity.TglDokumen =
                    request.TglDokumen;

                entity.TglManualJurnal =
                    request.TglManualJurnal;

                entity.TglPembatalan =
                    request.TglPembatalan;

                entity.TipeDokumen =
                    request.TipeDokumen?.Trim();

                entity.TempRJId =
                    request.TempRJId;

                entity.RecurringJournalName =
                    recurringJournal?
                        .RecurringJournalName;

                entity.RecurringJournalDate =
                    recurringJournal?
                        .RecurringJournalDate;

                entity.MataUangId =
                    request.MataUangId;

                entity.NamaMataUang =
                    request.NamaMataUang?.Trim();

                entity.ExchangeRateId =
                    request.ExchangeRateId;

                entity.RateToIdr =
                    request.RateToIdr;

                entity.UnbalancedAmount =
                    request.UnbalancedAmount;

                entity.Keterangan =
                    request.Keterangan?.Trim();

                entity.UpdateBy =
                    userActiveId.Value;

                entity.UpdateDateTime =
                    DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    status = "success",
                    message =
                        "Jurnal manual berhasil diperbarui."
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

                var entity = await _context.AccManualJurnals
                    .FirstOrDefaultAsync(x =>
                        x.AccManualJurnalId == id &&
                        (x.IsDelete == false ||
                         x.IsDelete == null));

                if (entity == null)
                {
                    return NotFound(new
                    {
                        message =
                            "Jurnal manual tidak ditemukan."
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
                        "Jurnal manual berhasil dihapus."
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

        private async Task<string> GenerateKodeManualJurnal()
        {
            const string prefix = "ACC-MJ-";

            var lastCode = await _context.AccManualJurnals
                .AsNoTracking()
                .Where(x =>
                    x.KodeManualJurnal != null &&
                    x.KodeManualJurnal.StartsWith(prefix))
                .OrderByDescending(x =>
                    x.KodeManualJurnal)
                .Select(x =>
                    x.KodeManualJurnal)
                .FirstOrDefaultAsync();

            var nextNumber = 1;

            if (!string.IsNullOrWhiteSpace(lastCode))
            {
                var numberPart =
                    lastCode.Replace(prefix, "");

                if (int.TryParse(
                    numberPart,
                    out var lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber:D5}";
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