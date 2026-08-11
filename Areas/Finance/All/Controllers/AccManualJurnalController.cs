using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.Finance.All.Models;
using QuilvianSystemBackendDev.Areas.Finance.All.ViewModels;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.Finance.All.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class AccManualJurnalController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly ILogger<AccManualJurnalController> _logger;

        public AccManualJurnalController(
            ApplicationDbContext applicationDbContext,
            ILogger<AccManualJurnalController> logger)
        {
            _applicationDbContext = applicationDbContext;
            _logger = logger;
        }

        // =====================================================
        // PAGED
        // =====================================================

        [HttpGet("paged")]
        public async Task<IActionResult> PagedAccManualJurnal(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "TglDokumen",
            string? sortDirection = "desc",
            string? tipeDokumen = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                if (page < 1)
                    page = 1;

                if (perPage < 1)
                    perPage = 10;

                var query = _applicationDbContext
                    .AccManualJurnals
                    .AsNoTracking()
                    .AsQueryable();

                // SEARCH
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

                // FILTER TIPE DOKUMEN
                if (!string.IsNullOrWhiteSpace(tipeDokumen))
                {
                    query = query.Where(x =>
                        x.TipeDokumen == tipeDokumen);
                }

                // FILTER TANGGAL
                if (startDate.HasValue)
                {
                    query = query.Where(x =>
                        x.TglDokumen >= startDate.Value.Date);
                }

                if (endDate.HasValue)
                {
                    var endExclusive =
                        endDate.Value.Date.AddDays(1);

                    query = query.Where(x =>
                        x.TglDokumen < endExclusive);
                }

                // SORTING
                var sortColumn =
                    orderBy?.Trim().ToLower()
                    ?? "tgldokumen";

                var isDescending =
                    sortDirection?.Trim().ToLower()
                    == "desc";

                query = sortColumn switch
                {
                    "kodemanualjurnal" =>
                        isDescending
                            ? query.OrderByDescending(x =>
                                x.KodeManualJurnal)
                            : query.OrderBy(x =>
                                x.KodeManualJurnal),

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

                    "namamatauang" =>
                        isDescending
                            ? query.OrderByDescending(x =>
                                x.NamaMataUang)
                            : query.OrderBy(x =>
                                x.NamaMataUang),

                    "unbalancedamount" =>
                        isDescending
                            ? query.OrderByDescending(x =>
                                x.UnbalancedAmount)
                            : query.OrderBy(x =>
                                x.UnbalancedAmount),

                    _ =>
                        isDescending
                            ? query.OrderByDescending(x =>
                                x.TglDokumen)
                            : query.OrderBy(x =>
                                x.TglDokumen)
                };

                var totalRows = await query.CountAsync();

                var totalPages =
                    (int)Math.Ceiling(
                        totalRows / (double)perPage);

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

                        x.Keterangan
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

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var data = await _applicationDbContext
                    .AccManualJurnals
                    .AsNoTracking()
                    .Where(x =>
                        x.AccManualJurnalId == id)
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
        // CREATE
        // LANGSUNG PUSH KE AccManualJurnals
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] AccManualJurnalViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (vm.TempRJId.HasValue)
                {
                    var exists = await _applicationDbContext.RecurringJournals
                        .AnyAsync(x => x.TempRJId == vm.TempRJId.Value);

                    if (!exists)
                    {
                        return BadRequest(new
                        {
                            message = "Recurring Journal tidak ditemukan."
                        });
                    }
                }
                var data = new AccManualJurnal
                {
                    AccManualJurnalId = Guid.NewGuid(),

                    KodeManualJurnal =
                        vm.KodeManualJurnal,

                    TglDokumen =
                        vm.TglDokumen,

                    TglManualJurnal =
                        vm.TglManualJurnal,

                    TglPembatalan =
                        vm.TglPembatalan,

                    TipeDokumen =
                        vm.TipeDokumen,

                    TempRJId =
                        vm.TempRJId,

                    RecurringJournalName =
                        vm.RecurringJournalName,

                    RecurringJournalDate =
                        vm.RecurringJournalDate,

                    MataUangId =
                        vm.MataUangId,

                    NamaMataUang =
                        vm.NamaMataUang,

                    ExchangeRateId =
                        vm.ExchangeRateId,

                    RateToIdr =
                        vm.RateToIdr,

                    UnbalancedAmount =
                        vm.UnbalancedAmount,

                    Keterangan =
                        vm.Keterangan
                };

                _applicationDbContext
                    .AccManualJurnals
                    .Add(data);

                var result =
                    await _applicationDbContext
                        .SaveChangesAsync();

                if (result > 0)
                {
                    return Created("", new
                    {
                        message = "Manual Jurnal berhasil dibuat.",
                        AccManualJurnalId = data.AccManualJurnalId
                    });
                }

                return StatusCode(500, new
                {
                    message =
                        "Gagal menyimpan data."
                });
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());

                return StatusCode(500, new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        // =====================================================
        // UPDATE
        // =====================================================

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] AccManualJurnalViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var data = await _applicationDbContext
                    .AccManualJurnals
                    .FirstOrDefaultAsync(x =>
                        x.AccManualJurnalId == id);

                if (data == null)
                {
                    return NotFound(new
                    {
                        message = "Data tidak ditemukan."
                    });
                }

                data.KodeManualJurnal =
                    vm.KodeManualJurnal;

                data.TglDokumen =
                    vm.TglDokumen;

                data.TglManualJurnal =
                    vm.TglManualJurnal;

                data.TglPembatalan =
                    vm.TglPembatalan;

                data.TipeDokumen =
                    vm.TipeDokumen;

                data.TempRJId =
                    vm.TempRJId;

                data.RecurringJournalName =
                    vm.RecurringJournalName;

                data.RecurringJournalDate =
                    vm.RecurringJournalDate;

                data.MataUangId =
                    vm.MataUangId;

                data.NamaMataUang =
                    vm.NamaMataUang;

                data.ExchangeRateId =
                    vm.ExchangeRateId;

                data.RateToIdr =
                    vm.RateToIdr;

                data.UnbalancedAmount =
                    vm.UnbalancedAmount;

                data.Keterangan =
                    vm.Keterangan;

                _applicationDbContext
                    .AccManualJurnals
                    .Update(data);

                var result =
                    await _applicationDbContext
                        .SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new
                    {
                        message =
                            "Update data berhasil."
                    });
                }

                return StatusCode(500, new
                {
                    message =
                        "Gagal update data."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());

                return StatusCode(500, new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        // =====================================================
        // DELETE LANGSUNG
        // =====================================================

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var data = await _applicationDbContext
                    .AccManualJurnals
                    .FirstOrDefaultAsync(x =>
                        x.AccManualJurnalId == id);

                if (data == null)
                {
                    return NotFound(new
                    {
                        message = "Data tidak ditemukan."
                    });
                }

                _applicationDbContext
                    .AccManualJurnals
                    .Remove(data);

                var result =
                    await _applicationDbContext
                        .SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new
                    {
                        message =
                            "Delete berhasil."
                    });
                }

                return StatusCode(500, new
                {
                    message =
                        "Gagal delete data."
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