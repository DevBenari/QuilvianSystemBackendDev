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
using System.Data;
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

        private async Task<string> GenerateKodePurchasingInvoiceAsync()
        {
            var prefix = $"AP-{DateTime.Now:yy}-";

            var lastNo =
                await _applicationDbContext.DetailTukarFakturs
                .AsNoTracking()
                .Where(x =>
                    x.IsDelete == false &&
                    x.KodePurchasingInvoice != null &&
                    x.KodePurchasingInvoice.StartsWith(prefix))
                .OrderByDescending(x => x.KodePurchasingInvoice)
                .Select(x => x.KodePurchasingInvoice)
                .FirstOrDefaultAsync();

            var nextNumber = 1;

            if (!string.IsNullOrWhiteSpace(lastNo) && lastNo.Length > prefix.Length)
            {
                var numberPart = lastNo.Substring(prefix.Length);

                if (int.TryParse(numberPart, out var lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber:D6}";
        }

        private async Task<string> GenerateNoInvoiceAsync()
        {
            var prefix = DateTime.Now.ToString("yyMMdd");

            var lastNo =
                await _applicationDbContext.DetailTukarFakturs
                .AsNoTracking()
                .Where(x =>
                    x.IsDelete == false &&
                    x.NoInvoice != null &&
                    x.NoInvoice.StartsWith(prefix))
                .OrderByDescending(x => x.NoInvoice)
                .Select(x => x.NoInvoice)
                .FirstOrDefaultAsync();

            var nextNumber = 1;

            if (!string.IsNullOrWhiteSpace(lastNo) && lastNo.Length > prefix.Length)
            {
                var numberPart = lastNo.Substring(prefix.Length);

                if (int.TryParse(numberPart, out var lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber:D6}";
        }

        private class POInfoDto
        {
            public Guid POId { get; set; }
            public Guid SupplierId { get; set; }
            public string NomorPO { get; set; } = string.Empty;
            public string? SupplierName { get; set; }
            public string? SupplierCode { get; set; }
        }

        private async Task<POInfoDto?> GetPOInfoByIdAsync(Guid poId)
        {
            var data =
                await _applicationDbContext.PurchaseOrders
                .AsNoTracking()
                .Where(x =>
                    x.PurchaseOrderId == poId &&
                    x.IsDelete == false &&
                    x.SupplierId != null)
                .Select(x => new POInfoDto
                {
                    POId = x.PurchaseOrderId,
                    SupplierId = x.SupplierId!.Value,
                    NomorPO = x.PurchaseOrderNumber ?? string.Empty,
                    SupplierName = x.SupplierName,
                    SupplierCode = x.SupplierCode
                })
                .FirstOrDefaultAsync();

            return data;
        }

        private async Task SyncTotalInvoiceAPAsync(Guid tukarFakturId)
        {
            var header =
                await _applicationDbContext.TukarFakturs
                .FirstOrDefaultAsync(x =>
                    x.TukarFakturId == tukarFakturId &&
                    x.IsDelete == false);

            if (header == null)
                return;

            var totalAP =
                await _applicationDbContext.DetailTukarFakturs
                .Where(x =>
                    x.TukarFakturId == tukarFakturId &&
                    x.IsDelete == false)
                .SumAsync(x => (decimal?)x.NilaiPurchasingInvoice) ?? 0;

            header.TotalInvoiceAP = totalAP;
            header.UpdateDateTime = DateTime.UtcNow;

            _applicationDbContext.TukarFakturs.Update(header);
        }


        [HttpGet("paged")]
        public async Task<IActionResult> PagedDetailTukarFaktur(
        int page = 1,
        int perPage = 10,
        string? search = null,
        string? orderBy = "TglPembuatanInvoice",
        string? sortDirection = "desc",
        Guid? tukarFakturId = null,
        Guid? supplierId = null,
        Guid? poId = null,

        [FromQuery(Name = "NomorPO")]
        string? nomorPO = null,

        [FromQuery(Name = "KodePOInv")]
        string? kodePOInv = null,

        [FromQuery(Name = "KodePurchasingInvoice")]
        string? kodePurchasingInvoice = null,

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

                if (perPage > 200)
                    perPage = 200;

                var query =
                    from d in _applicationDbContext.DetailTukarFakturs
                        .AsNoTracking()

                    join po in _applicationDbContext.PurchaseOrders.AsNoTracking()
                        on d.POId equals (Guid?)po.PurchaseOrderId into poJoin
                    from po in poJoin.DefaultIfEmpty()

                    join h in _applicationDbContext.TukarFakturs
                        .AsNoTracking()
                    on d.TukarFakturId equals h.TukarFakturId

                    where d.IsDelete == false &&
                          h.IsDelete == false

                    select new
                    {
                        d.DetailTukarFakturId,
                        d.TukarFakturId,

                        h.NoTukarFaktur,

                        d.TglPembuatanInvoice,
                        d.KodePurchasingInvoice,

                        d.POId,
                        d.SupplierId,

                        NamaSupplier =
                            _applicationDbContext.Suppliers
                                .Where(s => s.SupplierId == d.SupplierId)
                                .Select(s => s.SupplierName)
                                .FirstOrDefault(),

                        d.NomorPO,
                        d.NoInvoice,
                        d.NilaiPurchasingInvoice,

                        h.TglJatuhTempo,

                        d.StatusInvoice,
                        d.Keterangan,

                        HeaderSupplierId = h.SupplierId,
                        h.TglRegistrasi,
                        h.TglTerimaFaktur,
                        po.RequestType
                    };

                // =========================
                // Search global
                // =========================
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var pattern = $"%{search.Trim()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.NoTukarFaktur ?? "", pattern) ||
                        EF.Functions.ILike(x.KodePurchasingInvoice ?? "", pattern) ||
                        EF.Functions.ILike(x.NamaSupplier ?? "", pattern) ||
                        EF.Functions.ILike(x.NomorPO ?? "", pattern) ||
                        EF.Functions.ILike(x.NoInvoice ?? "", pattern) ||
                        EF.Functions.ILike(x.StatusInvoice ?? "", pattern) ||
                        EF.Functions.ILike(x.Keterangan ?? "", pattern)
                    );
                }

                // =========================
                // Filter khusus Nomor PO
                // Query: ?NomorPO=PO-001
                // =========================
                if (!string.IsNullOrWhiteSpace(nomorPO))
                {
                    var pattern = $"%{nomorPO.Trim()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.NomorPO ?? "", pattern)
                    );
                }

                // =========================
                // Filter Kode PO Invoice
                // Query: ?KodePOInv=xxx
                // atau: ?KodePurchasingInvoice=xxx
                // =========================
                var kodePOInvFilter = !string.IsNullOrWhiteSpace(kodePOInv)
                    ? kodePOInv
                    : kodePurchasingInvoice;

                if (!string.IsNullOrWhiteSpace(kodePOInvFilter))
                {
                    var pattern = $"%{kodePOInvFilter.Trim()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.KodePurchasingInvoice ?? "", pattern)
                    );
                }

                if (tukarFakturId.HasValue)
                {
                    query = query.Where(x =>
                        x.TukarFakturId == tukarFakturId.Value);
                }

                if (supplierId.HasValue)
                {
                    query = query.Where(x =>
                        x.SupplierId == supplierId.Value);
                }

                if (poId.HasValue)
                {
                    query = query.Where(x =>
                        x.POId == poId.Value);
                }

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
                        x.TglPembuatanInvoice >= startUtc &&
                        x.TglPembuatanInvoice <= endUtc);
                }

                var sortColumn =
                    orderBy?.ToLower() ?? "tglpembuataninvoice";

                var isDescending =
                    sortDirection?.ToLower() == "desc";

                query = sortColumn switch
                {
                    "notukarfaktur" =>
                        isDescending
                            ? query.OrderByDescending(x => x.NoTukarFaktur)
                            : query.OrderBy(x => x.NoTukarFaktur),

                    "tglpembuataninvoice" =>
                        isDescending
                            ? query.OrderByDescending(x => x.TglPembuatanInvoice)
                            : query.OrderBy(x => x.TglPembuatanInvoice),

                    "kodepurchasinginvoice" =>
                        isDescending
                            ? query.OrderByDescending(x => x.KodePurchasingInvoice)
                            : query.OrderBy(x => x.KodePurchasingInvoice),

                    "kodepoinv" =>
                        isDescending
                            ? query.OrderByDescending(x => x.KodePurchasingInvoice)
                            : query.OrderBy(x => x.KodePurchasingInvoice),

                    "namasupplier" =>
                        isDescending
                            ? query.OrderByDescending(x => x.NamaSupplier)
                            : query.OrderBy(x => x.NamaSupplier),

                    "nomorpo" =>
                        isDescending
                            ? query.OrderByDescending(x => x.NomorPO)
                            : query.OrderBy(x => x.NomorPO),

                    "noinvoice" =>
                        isDescending
                            ? query.OrderByDescending(x => x.NoInvoice)
                            : query.OrderBy(x => x.NoInvoice),

                    "nilaipurchasinginvoice" =>
                        isDescending
                            ? query.OrderByDescending(x => x.NilaiPurchasingInvoice)
                            : query.OrderBy(x => x.NilaiPurchasingInvoice),

                    "tgljatuhtempo" =>
                        isDescending
                            ? query.OrderByDescending(x => x.TglJatuhTempo)
                            : query.OrderBy(x => x.TglJatuhTempo),

                    "statusinvoice" =>
                        isDescending
                            ? query.OrderByDescending(x => x.StatusInvoice)
                            : query.OrderBy(x => x.StatusInvoice),

                    _ =>
                        query.OrderByDescending(x => x.TglPembuatanInvoice)
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


                        join po in _applicationDbContext.PurchaseOrders.AsNoTracking()
                            on d.POId equals (Guid?)po.PurchaseOrderId into poJoin
                        from po in poJoin.DefaultIfEmpty()

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

                            h.NoTukarFaktur,

                            d.TglPembuatanInvoice,
                            d.KodePurchasingInvoice,

                            d.POId,
                            d.SupplierId,

                            NamaSupplier =
                                _applicationDbContext.Suppliers
                                .Where(s => s.SupplierId == d.SupplierId)
                                .Select(s => s.SupplierName)
                                .FirstOrDefault(),

                            d.NomorPO,
                            d.NoInvoice,
                            d.NilaiPurchasingInvoice,

                            h.TglJatuhTempo,

                            d.StatusInvoice,
                            d.Keterangan,

                            h.TglRegistrasi,
                            h.TglTerimaFaktur,
                            po.RequestType
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
            using var transaction =
                await _applicationDbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var header =
                    await _applicationDbContext.TukarFakturs
                    .FirstOrDefaultAsync(x =>
                        x.TukarFakturId == vm.TukarFakturId &&
                        x.IsDelete == false);

                if (header == null)
                {
                    return NotFound(new
                    {
                        message = "Tukar Faktur tidak ditemukan."
                    });
                }

                var poInfo =
                    await GetPOInfoByIdAsync(vm.POId);

                if (poInfo == null)
                {
                    return NotFound(new
                    {
                        message = "PO tidak ditemukan."
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

                var kodePurchasingInvoice =
                    await GenerateKodePurchasingInvoiceAsync();

                var noInvoice =
                    await GenerateNoInvoiceAsync();

                var data = new DetailTukarFaktur
                {
                    DetailTukarFakturId = Guid.NewGuid(),
                    TukarFakturId = vm.TukarFakturId,

                    TglPembuatanInvoice = vm.TglPembuatanInvoice,

                    KodePurchasingInvoice = kodePurchasingInvoice,

                    POId = vm.POId,
                    SupplierId = poInfo.SupplierId,
                    NomorPO = poInfo.NomorPO,

                    NoInvoice = noInvoice,

                    NilaiPurchasingInvoice = vm.NilaiPurchasingInvoice,

                    StatusInvoice = "approved",

                    Keterangan = vm.Keterangan,

                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = userActiveId.Value,
                    IsDelete = false
                };

                _applicationDbContext.DetailTukarFakturs.Add(data);

                await _applicationDbContext.SaveChangesAsync();

                await SyncTotalInvoiceAPAsync(vm.TukarFakturId);

                await _applicationDbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return Created("", new
                {
                    message = "Tambah data berhasil.",
                    data = new
                    {
                        data.DetailTukarFakturId,
                        data.TukarFakturId,
                        header.NoTukarFaktur,
                        data.KodePurchasingInvoice,
                        data.NoInvoice,
                        data.POId,
                        data.SupplierId,
                        NamaSupplier = poInfo.SupplierName,
                        data.NomorPO,
                        data.NilaiPurchasingInvoice,
                        header.TglJatuhTempo,
                        data.StatusInvoice
                    }
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
        // UPDATE
        // =====================================================

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] DetailTukarFakturViewModel vm)
        {
            using var transaction =
                await _applicationDbContext.Database.BeginTransactionAsync();

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

                var header =
                    await _applicationDbContext.TukarFakturs
                    .FirstOrDefaultAsync(x =>
                        x.TukarFakturId == vm.TukarFakturId &&
                        x.IsDelete == false);

                if (header == null)
                {
                    return NotFound(new
                    {
                        message = "Tukar Faktur tidak ditemukan."
                    });
                }

                var poInfo =
                    await GetPOInfoByIdAsync(vm.POId);

                if (poInfo == null)
                {
                    return NotFound(new
                    {
                        message = "PO tidak ditemukan."
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

                var oldTukarFakturId =
                    data.TukarFakturId;

                data.TukarFakturId = vm.TukarFakturId;
                data.TglPembuatanInvoice = vm.TglPembuatanInvoice;

                data.POId = vm.POId;
                data.SupplierId = poInfo.SupplierId;
                data.NomorPO = poInfo.NomorPO;

                data.NilaiPurchasingInvoice = vm.NilaiPurchasingInvoice;
                data.Keterangan = vm.Keterangan;

                data.UpdateDateTime = DateTime.UtcNow;
                data.UpdateBy = userActiveId.Value;

                _applicationDbContext.DetailTukarFakturs.Update(data);

                await _applicationDbContext.SaveChangesAsync();

                await SyncTotalInvoiceAPAsync(vm.TukarFakturId);

                if (oldTukarFakturId != vm.TukarFakturId)
                {
                    await SyncTotalInvoiceAPAsync(oldTukarFakturId);
                }

                await _applicationDbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Update data berhasil."
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
        // DELETE
        // =====================================================

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            using var transaction =
                await _applicationDbContext.Database.BeginTransactionAsync();

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

                var tukarFakturId =
                    data.TukarFakturId;

                data.IsDelete = true;
                data.DeleteDateTime = DateTime.UtcNow;
                data.DeleteBy = userActiveId.Value;

                _applicationDbContext.DetailTukarFakturs.Update(data);

                await _applicationDbContext.SaveChangesAsync();

                await SyncTotalInvoiceAPAsync(tukarFakturId);

                await _applicationDbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Delete berhasil."
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