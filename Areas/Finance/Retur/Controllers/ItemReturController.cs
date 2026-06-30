using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.Finance.Retur.Models;
using QuilvianSystemBackendDev.Areas.Finance.Retur.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;
using System.Data;
using System.Security.Claims;

namespace QuilvianSystemBackendDev.Areas.Finance.Retur.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class ItemReturController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<ItemReturController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ItemReturController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ItemReturController> logger,
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

        private async Task RecalculateStatusReturAsync(Guid headerReturId, Guid userActiveId)
        {
            var header =
                await _applicationDbContext.HeaderReturs
                .FirstOrDefaultAsync(x =>
                    x.HeaderReturId == headerReturId &&
                    x.IsDelete == false);

            if (header == null)
                return;

            var items =
                await _applicationDbContext.ItemReturs
                .Where(x =>
                    x.HeaderReturId == headerReturId &&
                    x.IsDelete == false)
                .ToListAsync();

            if (!items.Any())
            {
                header.StatusRetur = "Outstanding";
                header.IsTerkonfirmasi = false;
            }
            else
            {
                var allFinished =
                    items.All(x =>
                        x.QtyTelahDiretur >= x.QtyRetur &&
                        x.QtyRetur > 0);

                header.StatusRetur =
                    allFinished ? "Selesai" : "Outstanding";

                header.IsTerkonfirmasi =
                    allFinished;
            }

            header.UpdateDateTime = DateTime.UtcNow;
            header.UpdateBy = userActiveId;

            foreach (var item in items)
            {
                item.StatusRetur = header.StatusRetur;
                item.IsTerkonfirmasi = header.IsTerkonfirmasi;
                item.TglRetur = header.TglRetur;
                item.UpdateDateTime = DateTime.UtcNow;
                item.UpdateBy = userActiveId;
            }

            await _applicationDbContext.SaveChangesAsync();
        }

        // =====================================================
        // PAGED ITEM RETUR
        // =====================================================

        [HttpGet("paged")]
        public async Task<IActionResult> PagedItemRetur(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "TglRetur",
            string? sortDirection = "desc",
            Guid? headerReturId = null,
            Guid? produkId = null,
            Guid? poId = null,
            Guid? receiveOrderId = null,
            string? statusRetur = null,
            bool? isTerkonfirmasi = null,

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

                var baseQuery =
                    _applicationDbContext.ItemReturs
                    .AsNoTracking()
                    .Where(x => x.IsDelete == false);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var keyword = $"%{search.Trim()}%";

                    baseQuery = baseQuery.Where(x =>
                        EF.Functions.ILike(x.NoBatch ?? "", keyword) ||
                        EF.Functions.ILike(x.NoFakturInvoice ?? "", keyword) ||
                        EF.Functions.ILike(x.NoPO ?? "", keyword) ||
                        EF.Functions.ILike(x.Satuan ?? "", keyword) ||
                        EF.Functions.ILike(x.Keterangan ?? "", keyword) ||

                        _applicationDbContext.HeaderReturs.Any(h =>
                            h.HeaderReturId == x.HeaderReturId &&
                            EF.Functions.ILike(h.KodeRetur ?? "", keyword)
                        ) ||

                        //_applicationDbContext.Products.Any(p =>
                        //    p.ProductId == x.ProdukId &&
                        //    EF.Functions.ILike(p.ProductName ?? "", keyword)
                        //) ||

                        _applicationDbContext.ReceiveOrders.Any(ro =>
                            ro.ReceiveOrderId == x.ReceiveOrderId &&
                            EF.Functions.ILike(ro.ReceiveOrderNumber ?? "", keyword)
                        )
                    );
                }

                if (headerReturId.HasValue)
                {
                    baseQuery = baseQuery.Where(x =>
                        x.HeaderReturId == headerReturId.Value);
                }

                if (produkId.HasValue)
                {
                    baseQuery = baseQuery.Where(x =>
                        x.ProdukId == produkId.Value);
                }

                if (poId.HasValue)
                {
                    baseQuery = baseQuery.Where(x =>
                        x.POId == poId.Value);
                }

                if (receiveOrderId.HasValue)
                {
                    baseQuery = baseQuery.Where(x =>
                        x.ReceiveOrderId == receiveOrderId.Value);
                }

                if (!string.IsNullOrWhiteSpace(statusRetur))
                {
                    var status = statusRetur.Trim();

                    baseQuery = baseQuery.Where(x =>
                        x.StatusRetur != null &&
                        x.StatusRetur.ToLower() == status.ToLower());
                }

                if (isTerkonfirmasi.HasValue)
                {
                    baseQuery = baseQuery.Where(x =>
                        x.IsTerkonfirmasi == isTerkonfirmasi.Value);
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

                    baseQuery = baseQuery.Where(x =>
                        x.TglRetur >= startUtc &&
                        x.TglRetur <= endUtc);
                }

                var query =
                    baseQuery.Select(x => new
                    {
                        x.ItemReturId,
                        x.ProdukId,

                        NamaProduk =
                            _applicationDbContext.Obats
                            .Where(p => p.ObatId == x.ProdukId)
                            .Select(p => p.ObatName)
                            .FirstOrDefault(),

                        x.HeaderReturId,

                        KodeRetur =
                            _applicationDbContext.HeaderReturs
                            .Where(h => h.HeaderReturId == x.HeaderReturId)
                            .Select(h => h.KodeRetur)
                            .FirstOrDefault(),

                        SupplierId =
                            _applicationDbContext.HeaderReturs
                            .Where(h => h.HeaderReturId == x.HeaderReturId)
                            .Select(h => h.SupplierId)
                            .FirstOrDefault(),

                        NamaSupplier =
                            _applicationDbContext.Suppliers
                            .Where(s => s.SupplierId ==
                                _applicationDbContext.HeaderReturs
                                    .Where(h => h.HeaderReturId == x.HeaderReturId)
                                    .Select(h => h.SupplierId)
                                    .FirstOrDefault()
                            )
                            .Select(s => s.SupplierName)
                            .FirstOrDefault(),

                        PPN =
                            _applicationDbContext.Suppliers
                            .Where(s => s.SupplierId ==
                                _applicationDbContext.HeaderReturs
                                    .Where(h => h.HeaderReturId == x.HeaderReturId)
                                    .Select(h => h.SupplierId)
                                    .FirstOrDefault()
                            )
                            .Select(s => s.PPN)
                            .FirstOrDefault(),

                        x.StatusRetur,
                        x.IsTerkonfirmasi,
                        x.TglRetur,
                        x.NoBatch,
                        x.NoFakturInvoice,
                        x.NoPO,
                        x.POId,
                        x.QtyDiterima,
                        x.QtyTelahDiretur,
                        x.ReceiveOrderId,

                        ReceiveNumber =
                            _applicationDbContext.ReceiveOrders
                            .Where(ro => ro.ReceiveOrderId == x.ReceiveOrderId)
                            .Select(ro => ro.ReceiveOrderNumber)
                            .FirstOrDefault(),

                        x.QtyRetur,
                        x.Satuan,
                        x.HargaSatuan,
                        x.SubtotalHarga,
                        x.TglPenerimaanPO,
                        x.TglTukarFaktur,
                        x.HargaTotal,
                        x.Keterangan
                    });

                var sortColumn =
                    orderBy?.ToLower() ?? "tglretur";

                var isDescending =
                    sortDirection?.ToLower() == "desc";

                query = sortColumn switch
                {
                    //"namaproduk" =>
                    //    isDescending
                    //        ? query.OrderByDescending(x => x.NamaProduk)
                    //        : query.OrderBy(x => x.NamaProduk),

                    "koderetur" =>
                        isDescending
                            ? query.OrderByDescending(x => x.KodeRetur)
                            : query.OrderBy(x => x.KodeRetur),

                    "nopo" =>
                        isDescending
                            ? query.OrderByDescending(x => x.NoPO)
                            : query.OrderBy(x => x.NoPO),

                    "receivenumber" =>
                        isDescending
                            ? query.OrderByDescending(x => x.ReceiveNumber)
                            : query.OrderBy(x => x.ReceiveNumber),

                    "qtyretur" =>
                        isDescending
                            ? query.OrderByDescending(x => x.QtyRetur)
                            : query.OrderBy(x => x.QtyRetur),

                    "hargasatuan" =>
                        isDescending
                            ? query.OrderByDescending(x => x.HargaSatuan)
                            : query.OrderBy(x => x.HargaSatuan),

                    "subtotalharga" =>
                        isDescending
                            ? query.OrderByDescending(x => x.SubtotalHarga)
                            : query.OrderBy(x => x.SubtotalHarga),

                    "tglretur" =>
                        isDescending
                            ? query.OrderByDescending(x => x.TglRetur)
                            : query.OrderBy(x => x.TglRetur),

                    _ =>
                        query.OrderByDescending(x => x.TglRetur)
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
        // GET BY ID ITEM
        // =====================================================

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var data =
                    await _applicationDbContext.ItemReturs
                    .AsNoTracking()
                    .Where(x =>
                        x.ItemReturId == id &&
                        x.IsDelete == false)
                    .Select(x => new
                    {
                        x.ItemReturId,
                        x.ProdukId,



                        SupplierId =
                            _applicationDbContext.HeaderReturs
                            .Where(h => h.HeaderReturId == x.HeaderReturId)
                            .Select(h => h.SupplierId)
                            .FirstOrDefault(),

                        NamaSupplier =
                            _applicationDbContext.HeaderReturs
                            .Where(h => h.HeaderReturId == x.HeaderReturId)
                            .Select(h => h.NamaSupplier)
                            .FirstOrDefault(),

                        x.HeaderReturId,

                        KodeRetur =
                            _applicationDbContext.HeaderReturs
                            .Where(h => h.HeaderReturId == x.HeaderReturId)
                            .Select(h => h.KodeRetur)
                            .FirstOrDefault(),

                        x.StatusRetur,
                        x.IsTerkonfirmasi,
                        x.TglRetur,
                        x.NoBatch,
                        x.NoFakturInvoice,
                        x.NoPO,
                        x.POId,
                        x.QtyDiterima,
                        x.QtyTelahDiretur,
                        x.ReceiveOrderId,
                        x.HargaTotal,

                        ReceiveNumber =
                            _applicationDbContext.ReceiveOrders
                            .Where(ro => ro.ReceiveOrderId == x.ReceiveOrderId)
                            .Select(ro => ro.ReceiveOrderNumber)
                            .FirstOrDefault(),

                        x.QtyRetur,
                        x.Satuan,
                        x.HargaSatuan,
                        x.SubtotalHarga,
                        x.TglPenerimaanPO,
                        x.TglTukarFaktur,
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
        // CREATE ITEM
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ItemReturViewModel vm)
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

                var header =
                    await _applicationDbContext.HeaderReturs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.HeaderReturId == vm.HeaderReturId &&
                        x.IsDelete == false);

                if (header == null)
                {
                    return BadRequest(new
                    {
                        message = "Header retur tidak ditemukan."
                    });
                }

                //var productExists =
                //    await _applicationDbContext.Products
                //    .AnyAsync(x =>
                //        x.ProductId == vm.ProdukId &&
                //        x.IsDelete == false);

                //if (!productExists)
                //{
                //    return BadRequest(new
                //    {
                //        message = "Produk tidak ditemukan."
                //    });
                //}

                var poExists =
                    await _applicationDbContext.PurchaseOrders
                    .AnyAsync(x =>
                        x.PurchaseOrderId == vm.POId &&
                        x.IsDelete == false);

                if (!poExists)
                {
                    return BadRequest(new
                    {
                        message = "Purchase Order tidak ditemukan."
                    });
                }

                var receiveOrderExists =
                    await _applicationDbContext.ReceiveOrders
                    .AnyAsync(x =>
                        x.ReceiveOrderId == vm.ReceiveOrderId &&
                        x.IsDelete == false);

                if (!receiveOrderExists)
                {
                    return BadRequest(new
                    {
                        message = "Receive Order tidak ditemukan."
                    });
                }

                if (vm.QtyRetur <= 0)
                {
                    return BadRequest(new
                    {
                        message = "Qty retur harus lebih dari 0."
                    });
                }

                if (vm.QtyRetur > vm.QtyDiterima)
                {
                    return BadRequest(new
                    {
                        message = "Qty retur tidak boleh lebih besar dari qty diterima."
                    });
                }

                var itemId =
                    Guid.NewGuid();

                var subtotalHarga = vm.QtyRetur * vm.HargaSatuan;
                var hargaTotal = vm.QtyRetur * vm.HargaSatuan;

                var data = new ItemRetur
                {
                    ItemReturId = itemId,
                    ProdukId = vm.ProdukId,
                    HeaderReturId = vm.HeaderReturId,

                    StatusRetur = header.StatusRetur,
                    IsTerkonfirmasi = header.IsTerkonfirmasi,
                    TglRetur = header.TglRetur,

                    NoBatch = vm.NoBatch,
                    NoFakturInvoice = vm.NoFakturInvoice,
                    NoPO = vm.NoPO,
                    POId = vm.POId,

                    QtyDiterima = vm.QtyDiterima,
                    QtyTelahDiretur = vm.QtyTelahDiretur,

                    ReceiveOrderId = vm.ReceiveOrderId,
                    QtyRetur = vm.QtyRetur,
                    Satuan = vm.Satuan,
                    HargaSatuan = vm.HargaSatuan,
                    SubtotalHarga = subtotalHarga,

                    HargaTotal = hargaTotal,

                    TglPenerimaanPO = vm.TglPenerimaanPO,
                    TglTukarFaktur = vm.TglTukarFaktur,
                    Keterangan = vm.Keterangan,

                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = userActiveId.Value,
                    IsDelete = false
                };

                _applicationDbContext.ItemReturs.Add(data);

                await _applicationDbContext.SaveChangesAsync();

                await RecalculateStatusReturAsync(vm.HeaderReturId, userActiveId.Value);

                await transaction.CommitAsync();

                return Created("", new
                {
                    message = "Tambah data berhasil.",
                    data = new
                    {
                        ItemReturId = itemId,
                        HeaderReturId = vm.HeaderReturId
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
        // UPDATE ITEM
        // =====================================================

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ItemReturViewModel vm)
        {
            using var transaction =
                await _applicationDbContext.Database.BeginTransactionAsync();

            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var data =
                    await _applicationDbContext.ItemReturs
                    .FirstOrDefaultAsync(x =>
                        x.ItemReturId == id &&
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

                var header =
                    await _applicationDbContext.HeaderReturs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.HeaderReturId == vm.HeaderReturId &&
                        x.IsDelete == false);

                if (header == null)
                {
                    return BadRequest(new
                    {
                        message = "Header retur tidak ditemukan."
                    });
                }

                if (vm.QtyRetur <= 0)
                {
                    return BadRequest(new
                    {
                        message = "Qty retur harus lebih dari 0."
                    });
                }

                if (vm.QtyRetur > vm.QtyDiterima)
                {
                    return BadRequest(new
                    {
                        message = "Qty retur tidak boleh lebih besar dari qty diterima."
                    });
                }

                var oldHeaderReturId =
                    data.HeaderReturId;

                data.ProdukId = vm.ProdukId;
                data.HeaderReturId = vm.HeaderReturId;

                data.StatusRetur = header.StatusRetur;
                data.IsTerkonfirmasi = header.IsTerkonfirmasi;
                data.TglRetur = header.TglRetur;

                data.NoBatch = vm.NoBatch;
                data.NoFakturInvoice = vm.NoFakturInvoice;
                data.NoPO = vm.NoPO;
                data.POId = vm.POId;

                data.QtyDiterima = vm.QtyDiterima;
                data.QtyTelahDiretur = vm.QtyTelahDiretur;

                data.ReceiveOrderId = vm.ReceiveOrderId;
                data.QtyRetur = vm.QtyRetur;
                data.Satuan = vm.Satuan;
                data.HargaSatuan = vm.HargaSatuan;
                data.SubtotalHarga = vm.QtyRetur * vm.HargaSatuan;

                data.TglPenerimaanPO = vm.TglPenerimaanPO;
                data.TglTukarFaktur = vm.TglTukarFaktur;
                data.Keterangan = vm.Keterangan;

                data.UpdateDateTime = DateTime.UtcNow;
                data.UpdateBy = userActiveId.Value;

                _applicationDbContext.ItemReturs.Update(data);

                await _applicationDbContext.SaveChangesAsync();

                await RecalculateStatusReturAsync(vm.HeaderReturId, userActiveId.Value);

                if (oldHeaderReturId != vm.HeaderReturId)
                {
                    await RecalculateStatusReturAsync(oldHeaderReturId, userActiveId.Value);
                }

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
        // DELETE ITEM
        // =====================================================

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            using var transaction =
                await _applicationDbContext.Database.BeginTransactionAsync();

            try
            {
                var data =
                    await _applicationDbContext.ItemReturs
                    .FirstOrDefaultAsync(x =>
                        x.ItemReturId == id &&
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

                var headerReturId =
                    data.HeaderReturId;

                data.IsDelete = true;
                data.DeleteDateTime = DateTime.UtcNow;
                data.DeleteBy = userActiveId.Value;

                _applicationDbContext.ItemReturs.Update(data);

                await _applicationDbContext.SaveChangesAsync();

                await RecalculateStatusReturAsync(headerReturId, userActiveId.Value);

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