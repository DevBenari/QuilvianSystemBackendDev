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
    public class RecurringJournalController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly ILogger<RecurringJournalController> _logger;

        public RecurringJournalController(
            ApplicationDbContext applicationDbContext,
            ILogger<RecurringJournalController> logger)
        {
            _applicationDbContext = applicationDbContext;
            _logger = logger;
        }

        // =====================================================
        // PAGED
        // =====================================================

        [HttpGet("paged")]
        public async Task<IActionResult> PagedRecurringJournal(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "RecurringJournalDate",
            string? sortDirection = "desc")
        {
            try
            {
                if (page < 1)
                    page = 1;

                if (perPage < 1)
                    perPage = 10;

                var query = _applicationDbContext
                    .RecurringJournals
                    .AsNoTracking()
                    .AsQueryable();

                // SEARCH
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

                // SORTING
                var sortColumn =
                    orderBy?.Trim().ToLower()
                    ?? "recurringjournaldate";

                var isDescending =
                    sortDirection?.Trim().ToLower()
                    == "desc";

                query = sortColumn switch
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
                                x.RecurringJournalDate)
                            : query.OrderBy(x =>
                                x.RecurringJournalDate)
                };

                // PAGINATION
                var totalRows = await query.CountAsync();

                var totalPages =
                    (int)Math.Ceiling(
                        totalRows / (double)perPage);

                var rows = await query
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .Select(x => new
                    {
                        x.TempRJId,
                        x.RecurringJournalName,
                        x.RecurringJournalDate,
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
                _logger.LogError(ex, ex.ToString());

                return StatusCode(500, new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message
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
                    .RecurringJournals
                    .AsNoTracking()
                    .Where(x =>
                        x.TempRJId == id)
                    .Select(x => new
                    {
                        x.TempRJId,
                        x.RecurringJournalName,
                        x.RecurringJournalDate,
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
                _logger.LogError(ex, ex.ToString());

                return StatusCode(500, new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        // =====================================================
        // CREATE
        // LANGSUNG PUSH KE RecurringJournals
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] RecurringJournalViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var data = new RecurringJournal
                {
                    TempRJId = Guid.NewGuid(),

                    RecurringJournalName =
                        vm.RecurringJournalName,

                    RecurringJournalDate =
                        vm.RecurringJournalDate,

                    Keterangan =
                        vm.Keterangan
                };

                _applicationDbContext
                    .RecurringJournals
                    .Add(data);

                var result =
                    await _applicationDbContext
                        .SaveChangesAsync();

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
            [FromBody] RecurringJournalViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var data = await _applicationDbContext
                    .RecurringJournals
                    .FirstOrDefaultAsync(x =>
                        x.TempRJId == id);

                if (data == null)
                {
                    return NotFound(new
                    {
                        message = "Data tidak ditemukan."
                    });
                }

                data.RecurringJournalName =
                    vm.RecurringJournalName;

                data.RecurringJournalDate =
                    vm.RecurringJournalDate;

                data.Keterangan =
                    vm.Keterangan;

                _applicationDbContext
                    .RecurringJournals
                    .Update(data);

                var result =
                    await _applicationDbContext
                        .SaveChangesAsync();

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
                    .RecurringJournals
                    .FirstOrDefaultAsync(x =>
                        x.TempRJId == id);

                if (data == null)
                {
                    return NotFound(new
                    {
                        message = "Data tidak ditemukan."
                    });
                }

                _applicationDbContext
                    .RecurringJournals
                    .Remove(data);

                var result =
                    await _applicationDbContext
                        .SaveChangesAsync();

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
                _logger.LogError(ex, ex.ToString());

                return StatusCode(500, new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }
    }
}