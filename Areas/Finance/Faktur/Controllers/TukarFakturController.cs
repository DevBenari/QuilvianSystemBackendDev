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
    public class TukarFakturController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<TukarFakturController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TukarFakturController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<TukarFakturController> logger,
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
        // PAGED HEADER
        // =====================================================

        [HttpGet("paged")]
        public async Task<IActionResult> PagedTukarFaktur(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "TglRegistrasi",
            string? sortDirection = "desc",
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

                var baseQuery =
                    _applicationDbContext.TukarFakturs
                    .AsNoTracking()
                    .Where(x => x.IsDelete == false);

                // SEARCH
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = $"%{search.Trim().ToLower()}%";

                    baseQuery = baseQuery.Where(x =>
                        EF.Functions.ILike(x.Keterangan ?? "", search) ||

                        _applicationDbContext.DetailTukarFakturs.Any(d =>
                            d.TukarFakturId == x.TukarFakturId &&
                            d.IsDelete == false &&
                            (
                                EF.Functions.ILike(d.NomorPO ?? "", search) ||
                                EF.Functions.ILike(d.NoInvoice ?? "", search) ||
                                EF.Functions.ILike(d.Keterangan ?? "", search)
                            )
                        )
                    );
                }

                // FILTER SUPPLIER
                if (supplierId.HasValue)
                {
                    baseQuery = baseQuery.Where(x =>
                        x.SupplierId == supplierId.Value);
                }

                // FILTER DATE BERDASARKAN TGL REGISTRASI
                if (startDate.HasValue && endDate.HasValue)
                {
                    DateTime startUtc =
                        startDate.Value.Date.ToUniversalTime();

                    DateTime endUtc =
                        endDate.Value.Date
                        .AddDays(1)
                        .AddTicks(-1)
                        .ToUniversalTime();

                    baseQuery = baseQuery.Where(x =>
                        x.TglRegistrasi >= startUtc &&
                        x.TglRegistrasi <= endUtc);
                }

                var query = baseQuery.Select(x => new
                {
                    x.TukarFakturId,
                    x.SupplierId,
                    x.TglRegistrasi,
                    x.TglTerimaFaktur,
                    x.TglJatuhTempo,
                    x.Keterangan,

                    JumlahDetail =
                        _applicationDbContext.DetailTukarFakturs
                        .Count(d =>
                            d.TukarFakturId == x.TukarFakturId &&
                            d.IsDelete == false),

                    TotalInvoice =
                        _applicationDbContext.DetailTukarFakturs
                        .Where(d =>
                            d.TukarFakturId == x.TukarFakturId &&
                            d.IsDelete == false)
                        .Sum(d => (decimal?)d.TotalInvoice) ?? 0
                });

                // SORTING
                var sortColumn =
                    orderBy?.ToLower() ?? "tglregistrasi";

                var isDescending =
                    sortDirection?.ToLower() == "desc";

                query = sortColumn switch
                {
                    "supplierid" =>
                        isDescending
                            ? query.OrderByDescending(x => x.SupplierId)
                            : query.OrderBy(x => x.SupplierId),

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

                    "totalinvoice" =>
                        isDescending
                            ? query.OrderByDescending(x => x.TotalInvoice)
                            : query.OrderBy(x => x.TotalInvoice),

                    _ =>
                        query.OrderByDescending(x => x.TglRegistrasi)
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
        // PAGED DETAIL
        // =====================================================

        [HttpGet("detail/paged")]
        public async Task<IActionResult> PagedDetailTukarFaktur(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "NoInvoice",
            string? sortDirection = "asc",
            Guid? tukarFakturId = null
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

                // FILTER HEADER
                if (tukarFakturId.HasValue)
                {
                    query = query.Where(x =>
                        x.TukarFakturId == tukarFakturId.Value);
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
        // GET BY ID HEADER + DETAIL
        // =====================================================

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var data =
                    await _applicationDbContext.TukarFakturs
                    .AsNoTracking()
                    .Where(x =>
                        x.TukarFakturId == id &&
                        x.IsDelete == false)
                    .Select(x => new
                    {
                        x.TukarFakturId,
                        x.SupplierId,
                        x.TglRegistrasi,
                        x.TglTerimaFaktur,
                        x.TglJatuhTempo,
                        x.Keterangan,

                        Details =
                            _applicationDbContext.DetailTukarFakturs
                            .AsNoTracking()
                            .Where(d =>
                                d.TukarFakturId == x.TukarFakturId &&
                                d.IsDelete == false)
                            .Select(d => new
                            {
                                d.DetailTukarFakturId,
                                d.TukarFakturId,
                                d.NomorPO,
                                d.NoInvoice,
                                d.TotalInvoice,
                                d.Keterangan
                            })
                            .ToList()
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
        // CREATE HEADER + DETAIL
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] TukarFakturViewModel vm)
        {
            using var transaction =
                await _applicationDbContext.Database.BeginTransactionAsync();

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
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

                var headerId =
                    Guid.NewGuid();

                var header = new TukarFaktur
                {
                    TukarFakturId = headerId,
                    SupplierId = vm.SupplierId,

                    TglRegistrasi = vm.TglRegistrasi,
                    TglTerimaFaktur = vm.TglTerimaFaktur,
                    TglJatuhTempo = vm.TglJatuhTempo,

                    Keterangan = vm.Keterangan,

                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = userActiveId.Value,
                    IsDelete = false
                };

                _applicationDbContext.TukarFakturs.Add(header);

                if (vm.Details != null && vm.Details.Any())
                {
                    foreach (var detailVm in vm.Details)
                    {
                        var detail = new DetailTukarFaktur
                        {
                            DetailTukarFakturId = Guid.NewGuid(),
                            TukarFakturId = headerId,

                            NomorPO = detailVm.NomorPO,
                            NoInvoice = detailVm.NoInvoice,
                            TotalInvoice = detailVm.TotalInvoice,
                            Keterangan = detailVm.Keterangan,

                            CreateDateTime = DateTime.UtcNow,
                            CreateBy = userActiveId.Value,
                            IsDelete = false
                        };

                        _applicationDbContext.DetailTukarFakturs.Add(detail);
                    }
                }

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
                            TukarFakturId = headerId
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
        // UPDATE HEADER + DETAIL
        // =====================================================

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] TukarFakturViewModel vm)
        {
            using var transaction =
                await _applicationDbContext.Database.BeginTransactionAsync();

            try
            {
                var data =
                    await _applicationDbContext.TukarFakturs
                    .FirstOrDefaultAsync(x =>
                        x.TukarFakturId == id &&
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

                data.SupplierId = vm.SupplierId;
                data.TglRegistrasi = vm.TglRegistrasi;
                data.TglTerimaFaktur = vm.TglTerimaFaktur;
                data.TglJatuhTempo = vm.TglJatuhTempo;
                data.Keterangan = vm.Keterangan;

                data.UpdateDateTime = DateTime.UtcNow;
                data.UpdateBy = userActiveId.Value;

                _applicationDbContext.TukarFakturs.Update(data);

                var existingDetails =
                    await _applicationDbContext.DetailTukarFakturs
                    .Where(x =>
                        x.TukarFakturId == id &&
                        x.IsDelete == false)
                    .ToListAsync();

                var incomingDetailIds =
                    vm.Details?
                    .Where(x => x.DetailTukarFakturId.HasValue)
                    .Select(x => x.DetailTukarFakturId!.Value)
                    .ToList() ?? new List<Guid>();

                // SOFT DELETE DETAIL YANG DIHAPUS DARI PAYLOAD
                foreach (var existingDetail in existingDetails)
                {
                    if (!incomingDetailIds.Contains(existingDetail.DetailTukarFakturId))
                    {
                        existingDetail.IsDelete = true;
                        existingDetail.DeleteDateTime = DateTime.UtcNow;
                        existingDetail.DeleteBy = userActiveId.Value;

                        _applicationDbContext.DetailTukarFakturs.Update(existingDetail);
                    }
                }

                // UPDATE / INSERT DETAIL
                if (vm.Details != null && vm.Details.Any())
                {
                    foreach (var detailVm in vm.Details)
                    {
                        if (detailVm.DetailTukarFakturId.HasValue)
                        {
                            var existingDetail =
                                existingDetails
                                .FirstOrDefault(x =>
                                    x.DetailTukarFakturId == detailVm.DetailTukarFakturId.Value);

                            if (existingDetail != null)
                            {
                                existingDetail.NomorPO = detailVm.NomorPO;
                                existingDetail.NoInvoice = detailVm.NoInvoice;
                                existingDetail.TotalInvoice = detailVm.TotalInvoice;
                                existingDetail.Keterangan = detailVm.Keterangan;

                                existingDetail.UpdateDateTime = DateTime.UtcNow;
                                existingDetail.UpdateBy = userActiveId.Value;

                                _applicationDbContext.DetailTukarFakturs.Update(existingDetail);
                            }
                        }
                        else
                        {
                            var newDetail = new DetailTukarFaktur
                            {
                                DetailTukarFakturId = Guid.NewGuid(),
                                TukarFakturId = id,

                                NomorPO = detailVm.NomorPO,
                                NoInvoice = detailVm.NoInvoice,
                                TotalInvoice = detailVm.TotalInvoice,
                                Keterangan = detailVm.Keterangan,

                                CreateDateTime = DateTime.UtcNow,
                                CreateBy = userActiveId.Value,
                                IsDelete = false
                            };

                            _applicationDbContext.DetailTukarFakturs.Add(newDetail);
                        }
                    }
                }

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
        // DELETE HEADER + DETAIL
        // =====================================================

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            using var transaction =
                await _applicationDbContext.Database.BeginTransactionAsync();

            try
            {
                var data =
                    await _applicationDbContext.TukarFakturs
                    .FirstOrDefaultAsync(x =>
                        x.TukarFakturId == id &&
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

                _applicationDbContext.TukarFakturs.Update(data);

                var details =
                    await _applicationDbContext.DetailTukarFakturs
                    .Where(x =>
                        x.TukarFakturId == id &&
                        x.IsDelete == false)
                    .ToListAsync();

                foreach (var detail in details)
                {
                    detail.IsDelete = true;
                    detail.DeleteDateTime = DateTime.UtcNow;
                    detail.DeleteBy = userActiveId.Value;

                    _applicationDbContext.DetailTukarFakturs.Update(detail);
                }

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
