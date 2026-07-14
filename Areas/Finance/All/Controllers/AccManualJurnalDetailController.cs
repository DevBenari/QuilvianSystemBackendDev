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
    public class AccManualJurnalDetailController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly ILogger<AccManualJurnalDetailController> _logger;

        public AccManualJurnalDetailController(
            ApplicationDbContext applicationDbContext,
            ILogger<AccManualJurnalDetailController> logger)
        {
            _applicationDbContext = applicationDbContext;
            _logger = logger;
        }

        // =====================================================
        // PAGED
        // LANGSUNG DARI TABEL AccManualJurnalDetails
        // =====================================================

        [HttpGet("paged")]
        public async Task<IActionResult> PagedAccManualJurnalDetail(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "COAName",
            string? sortDirection = "asc",
            Guid? accManualJurnalId = null,
            Guid? coaId = null,
            Guid? kunjunganId = null,
            Guid? costCenterId = null)
        {
            try
            {
                if (page < 1)
                    page = 1;

                if (perPage < 1)
                    perPage = 10;

                var query = _applicationDbContext
                    .AccManualJurnalDetails
                    .AsNoTracking()
                    .AsQueryable();

                // SEARCH
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

                // FILTER MANUAL JURNAL
                if (accManualJurnalId.HasValue)
                {
                    query = query.Where(x =>
                        x.AccManualJurnalId ==
                        accManualJurnalId.Value);
                }

                // FILTER COA
                if (coaId.HasValue)
                {
                    query = query.Where(x =>
                        x.COAId == coaId.Value);
                }

                // FILTER KUNJUNGAN
                if (kunjunganId.HasValue)
                {
                    query = query.Where(x =>
                        x.KunjunganId ==
                        kunjunganId.Value);
                }

                // FILTER COST CENTER
                if (costCenterId.HasValue)
                {
                    query = query.Where(x =>
                        x.CostCenterId ==
                        costCenterId.Value);
                }

                // SORTING
                var sortColumn =
                    orderBy?.Trim().ToLower()
                    ?? "coaname";

                var isDescending =
                    sortDirection?.Trim().ToLower()
                    == "desc";

                query = sortColumn switch
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

                    "rolesetupcoa" =>
                        isDescending
                            ? query.OrderByDescending(x =>
                                x.RoleSetupCOA)
                            : query.OrderBy(x =>
                                x.RoleSetupCOA),

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

                    "noregistrasi" =>
                        isDescending
                            ? query.OrderByDescending(x =>
                                x.NoRegistrasi)
                            : query.OrderBy(x =>
                                x.NoRegistrasi),

                    "costcentername" =>
                        isDescending
                            ? query.OrderByDescending(x =>
                                x.CostCenterName)
                            : query.OrderBy(x =>
                                x.CostCenterName),

                    _ =>
                        isDescending
                            ? query.OrderByDescending(x =>
                                x.COAName)
                            : query.OrderBy(x =>
                                x.COAName)
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
                        x.DetAccManualJurnalId,
                        x.AccManualJurnalId,
                        x.DetailTempRJId,

                        x.COAId,
                        x.COACode,
                        x.COAName,
                        x.RoleSetupCOA,

                        x.DebetAmount,
                        x.CreditAmount,

                        x.KunjunganId,
                        x.NoRegistrasi,

                        x.CostCenterId,
                        x.CostCenterName,

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
                    .AccManualJurnalDetails
                    .AsNoTracking()
                    .Where(x =>
                        x.DetAccManualJurnalId == id)
                    .Select(x => new
                    {
                        x.DetAccManualJurnalId,
                        x.AccManualJurnalId,
                        x.DetailTempRJId,

                        x.COAId,
                        x.COACode,
                        x.COAName,
                        x.RoleSetupCOA,

                        x.DebetAmount,
                        x.CreditAmount,

                        x.KunjunganId,
                        x.NoRegistrasi,

                        x.CostCenterId,
                        x.CostCenterName,

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
        // LANGSUNG PUSH KE AccManualJurnalDetails
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] AccManualJurnalDetailViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                // Cek AccManualJurnalId
                if (vm.AccManualJurnalId == Guid.Empty ||
                    !await _applicationDbContext.AccManualJurnals.AnyAsync(x =>
                        x.AccManualJurnalId == vm.AccManualJurnalId))
                {
                    return BadRequest(new
                    {
                        message = "AccManualJurnalId tidak valid atau tidak ditemukan."
                    });
                }

                // Cek DetailTempRJId hanya jika diisi
                if (vm.DetailTempRJId is Guid detailTempRJId &&
                    (detailTempRJId == Guid.Empty ||
                     !await _applicationDbContext.RecurringJournalDetails.AnyAsync(x =>
                         x.DetailTempRJId == detailTempRJId)))
                {
                    return BadRequest(new
                    {
                        message = "DetailTempRJId tidak valid atau tidak ditemukan."
                    });
                }

                // Cek COAId
                if (vm.COAId == Guid.Empty ||
                    !await _applicationDbContext.MasterCoas.AnyAsync(x =>
                        x.COAId == vm.COAId))
                {
                    return BadRequest(new
                    {
                        message = "COAId tidak valid atau tidak ditemukan."
                    });
                }

                // Cost Center
                if (vm.CostCenterId.HasValue &&
                    !await _applicationDbContext.CostCenters
                        .AnyAsync(x => x.CostCenterId == vm.CostCenterId &&
                                       (x.IsDelete == false || x.IsDelete == null)))
                {
                    return BadRequest(new
                    {
                        message = "Cost Center tidak ditemukan."
                    });
                }

                // Kunjungan
                if (vm.KunjunganId.HasValue &&
                    !await _applicationDbContext.Kunjungans
                        .AnyAsync(x => x.KunjunganID == vm.KunjunganId))
                {
                    return BadRequest(new
                    {
                        message = "Kunjungan tidak ditemukan."
                    });
                }

                var data = new AccManualJurnalDetail
                {
                    DetAccManualJurnalId = Guid.NewGuid(),

                    AccManualJurnalId =
                        vm.AccManualJurnalId,

                    DetailTempRJId =
                        vm.DetailTempRJId,

                    COAId =
                        vm.COAId,

                    COACode =
                        vm.COACode,

                    COAName =
                        vm.COAName,

                    RoleSetupCOA =
                        vm.RoleSetupCOA,

                    DebetAmount =
                        vm.DebetAmount,

                    CreditAmount =
                        vm.CreditAmount,

                    KunjunganId =
                        vm.KunjunganId,

                    NoRegistrasi =
                        vm.NoRegistrasi,

                    CostCenterId =
                        vm.CostCenterId,

                    CostCenterName =
                        vm.CostCenterName,

                    Keterangan =
                        vm.Keterangan
                };

                _applicationDbContext
                    .AccManualJurnalDetails
                    .Add(data);

                var result =
                    await _applicationDbContext
                        .SaveChangesAsync();

                if (result > 0)
                {
                    return Created("", new
                    {
                        message =
                            "Tambah data berhasil."
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
            [FromBody] AccManualJurnalDetailViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var data = await _applicationDbContext
                    .AccManualJurnalDetails
                    .FirstOrDefaultAsync(x =>
                        x.DetAccManualJurnalId == id);

                if (data == null)
                {
                    return NotFound(new
                    {
                        message = "Data tidak ditemukan."
                    });
                }

                data.AccManualJurnalId =
                    vm.AccManualJurnalId;

                data.DetailTempRJId =
                    vm.DetailTempRJId;

                data.COAId =
                    vm.COAId;

                data.COACode =
                    vm.COACode;

                data.COAName =
                    vm.COAName;

                data.RoleSetupCOA =
                    vm.RoleSetupCOA;

                data.DebetAmount =
                    vm.DebetAmount;

                data.CreditAmount =
                    vm.CreditAmount;

                data.KunjunganId =
                    vm.KunjunganId;

                data.NoRegistrasi =
                    vm.NoRegistrasi;

                data.CostCenterId =
                    vm.CostCenterId;

                data.CostCenterName =
                    vm.CostCenterName;

                data.Keterangan =
                    vm.Keterangan;

                _applicationDbContext
                    .AccManualJurnalDetails
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
                    .AccManualJurnalDetails
                    .FirstOrDefaultAsync(x =>
                        x.DetAccManualJurnalId == id);

                if (data == null)
                {
                    return NotFound(new
                    {
                        message = "Data tidak ditemukan."
                    });
                }

                _applicationDbContext
                    .AccManualJurnalDetails
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
                    message = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }
    }
}