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
    public class HeaderReturController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<HeaderReturController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HeaderReturController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<HeaderReturController> logger,
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

        private async Task<string> GenerateKodeReturAsync()
        {
            var prefix = $"RETUR-{DateTime.Now:yyyyMMdd}-";

            var lastCode =
                await _applicationDbContext.HeaderReturs
                .AsNoTracking()
                .Where(x =>
                    x.IsDelete == false &&
                    x.KodeRetur != null &&
                    x.KodeRetur.StartsWith(prefix))
                .OrderByDescending(x => x.KodeRetur)
                .Select(x => x.KodeRetur)
                .FirstOrDefaultAsync();

            var nextNumber = 1;

            if (!string.IsNullOrWhiteSpace(lastCode) && lastCode.Length > prefix.Length)
            {
                var numberPart =
                    lastCode.Substring(prefix.Length);

                if (int.TryParse(numberPart, out var lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber:D4}";
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
        // PAGED HEADER RETUR
        // =====================================================

        [HttpGet("paged")]
        public async Task<IActionResult> PagedHeaderRetur(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "TglRetur",
            string? sortDirection = "desc",
            Guid? supplierId = null,
            Guid? gudangId = null,
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
                    _applicationDbContext.HeaderReturs
                    .AsNoTracking()
                    .Where(x => x.IsDelete == false);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var keyword = $"%{search.Trim()}%";

                    baseQuery = baseQuery.Where(x =>
                        EF.Functions.ILike(x.KodeRetur ?? "", keyword) ||
                        EF.Functions.ILike(x.StatusRetur ?? "", keyword) ||
                        EF.Functions.ILike(x.Keterangan ?? "", keyword) ||

                        _applicationDbContext.Suppliers.Any(s =>
                            s.SupplierId == x.SupplierId &&
                            EF.Functions.ILike(s.SupplierName ?? "", keyword)
                        ) ||

                        _applicationDbContext.WarehouseLocations.Any(g =>
                            g.WarehouseLocationId == x.GudangId &&
                            EF.Functions.ILike(g.WarehouseLocationName ?? "", keyword)
                        )
                    );
                }

                if (supplierId.HasValue)
                {
                    baseQuery = baseQuery.Where(x =>
                        x.SupplierId == supplierId.Value);
                }

                if (gudangId.HasValue)
                {
                    baseQuery = baseQuery.Where(x =>
                        x.GudangId == gudangId.Value);
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
                        x.HeaderReturId,
                        x.SupplierId,

                        NamaSupplier =
                            _applicationDbContext.Suppliers
                            .Where(s => s.SupplierId == x.SupplierId)
                            .Select(s => s.SupplierName)
                            .FirstOrDefault(),

                        x.GudangId,

                        NamaGudang =
                            _applicationDbContext.WarehouseLocations
                            .Where(g => g.WarehouseLocationId == x.GudangId)
                            .Select(g => g.WarehouseLocationName)
                            .FirstOrDefault(),

                        x.KodeRetur,
                        x.StatusRetur,
                        x.IsTerkonfirmasi,
                        x.TglRetur,
                        x.Keterangan,

                        JumlahItem =
                            _applicationDbContext.ItemReturs
                            .Count(i =>
                                i.HeaderReturId == x.HeaderReturId &&
                                i.IsDelete == false),

                        TotalQtyRetur =
                            _applicationDbContext.ItemReturs
                            .Where(i =>
                                i.HeaderReturId == x.HeaderReturId &&
                                i.IsDelete == false)
                            .Sum(i => (decimal?)i.QtyRetur) ?? 0,

                        TotalHargaRetur =
                            _applicationDbContext.ItemReturs
                            .Where(i =>
                                i.HeaderReturId == x.HeaderReturId &&
                                i.IsDelete == false)
                            .Sum(i => (decimal?)i.SubtotalHarga) ?? 0
                    });

                var sortColumn =
                    orderBy?.ToLower() ?? "tglretur";

                var isDescending =
                    sortDirection?.ToLower() == "desc";

                query = sortColumn switch
                {
                    "koderetur" =>
                        isDescending
                            ? query.OrderByDescending(x => x.KodeRetur)
                            : query.OrderBy(x => x.KodeRetur),

                    "namasupplier" =>
                        isDescending
                            ? query.OrderByDescending(x => x.NamaSupplier)
                            : query.OrderBy(x => x.NamaSupplier),

                    "namagudang" =>
                        isDescending
                            ? query.OrderByDescending(x => x.NamaGudang)
                            : query.OrderBy(x => x.NamaGudang),

                    "statusretur" =>
                        isDescending
                            ? query.OrderByDescending(x => x.StatusRetur)
                            : query.OrderBy(x => x.StatusRetur),

                    "tglretur" =>
                        isDescending
                            ? query.OrderByDescending(x => x.TglRetur)
                            : query.OrderBy(x => x.TglRetur),

                    "totalqtyretur" =>
                        isDescending
                            ? query.OrderByDescending(x => x.TotalQtyRetur)
                            : query.OrderBy(x => x.TotalQtyRetur),

                    "totalhargaretur" =>
                        isDescending
                            ? query.OrderByDescending(x => x.TotalHargaRetur)
                            : query.OrderBy(x => x.TotalHargaRetur),

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
        // GET BY ID HEADER + ITEM
        // =====================================================

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var header =
                    await _applicationDbContext.HeaderReturs
                    .AsNoTracking()
                    .Where(x =>
                        x.HeaderReturId == id &&
                        x.IsDelete == false)
                    .Select(x => new
                    {
                        x.HeaderReturId,
                        x.SupplierId,

                        NamaSupplier =
                            _applicationDbContext.Suppliers
                            .Where(s => s.SupplierId == x.SupplierId)
                            .Select(s => s.SupplierName)
                            .FirstOrDefault(),

                        x.GudangId,

                        NamaGudang =
                            _applicationDbContext.WarehouseLocations
                            .Where(g => g.WarehouseLocationId == x.GudangId)
                            .Select(g => g.WarehouseLocationName)
                            .FirstOrDefault(),

                        x.KodeRetur,
                        x.StatusRetur,
                        x.IsTerkonfirmasi,
                        x.TglRetur,
                        x.Keterangan
                    })
                    .FirstOrDefaultAsync();

                if (header == null)
                {
                    return NotFound(new
                    {
                        message = "Data tidak ditemukan."
                    });
                }

                var items =
                    await _applicationDbContext.ItemReturs
                    .AsNoTracking()
                    .Where(x =>
                        x.HeaderReturId == id &&
                        x.IsDelete == false)
                    .Select(x => new
                    {
                        x.ItemReturId,
                        x.ProdukId,

                        //NamaProduk =
                        //    _applicationDbContext.Products
                        //    .Where(p => p.ProductId == x.ProdukId)
                        //    .Select(p => p.ProductName)
                        //    .FirstOrDefault(),

                        x.HeaderReturId,
                        header.KodeRetur,
                        header.StatusRetur,
                        header.IsTerkonfirmasi,
                        header.TglRetur,

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
                        x.Keterangan
                    })
                    .OrderByDescending(x => x.ItemReturId)
                    .ToListAsync();

                return Ok(new
                {
                    status = "success",
                    data = new
                    {
                        header.HeaderReturId,
                        header.SupplierId,
                        header.NamaSupplier,
                        header.GudangId,
                        header.NamaGudang,
                        header.KodeRetur,
                        header.StatusRetur,
                        header.IsTerkonfirmasi,
                        header.TglRetur,
                        header.Keterangan,
                        Items = items
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
        // CREATE HEADER
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] HeaderReturViewModel vm)
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

                //var supplierExists =
                //    await _applicationDbContext.Suppliers
                //    .AnyAsync(x =>
                //        x.SupplierId == vm.SupplierId &&
                //        x.IsDelete == false);

                //if (!supplierExists)
                //{
                //    return BadRequest(new
                //    {
                //        message = "Supplier tidak ditemukan."
                //    });
                //}

                //var gudangExists =
                //    await _applicationDbContext.WarehouseLocations
                //    .AnyAsync(x =>
                //        x.WarehouseLocationId == vm.GudangId &&
                //        x.IsDelete == false);

                //if (!gudangExists)
                //{
                //    return BadRequest(new
                //    {
                //        message = "Gudang tidak ditemukan."
                //    });
                //}

                var headerId =
                    Guid.NewGuid();

                var kodeRetur =
                    await GenerateKodeReturAsync();

                var data = new HeaderRetur
                {
                    HeaderReturId = headerId,
                    SupplierId = vm.SupplierId,
                    GudangId = (Guid)vm.GudangId,
                    KodeRetur = kodeRetur,
                    StatusRetur = vm.IsTerkonfirmasi ? "Selesai" : "Outstanding",
                    IsTerkonfirmasi = vm.IsTerkonfirmasi,
                    TglRetur = vm.TglRetur == default ? DateTime.UtcNow : vm.TglRetur,
                    Keterangan = vm.Keterangan,

                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = userActiveId.Value,
                    IsDelete = false
                };

                _applicationDbContext.HeaderReturs.Add(data);

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
                            HeaderReturId = headerId,
                            KodeRetur = kodeRetur
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
        // UPDATE HEADER
        // =====================================================

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] HeaderReturViewModel vm)
        {
            using var transaction =
                await _applicationDbContext.Database.BeginTransactionAsync();

            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var data =
                    await _applicationDbContext.HeaderReturs
                    .FirstOrDefaultAsync(x =>
                        x.HeaderReturId == id &&
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
                data.GudangId = (Guid)vm.GudangId;
                data.TglRetur = vm.TglRetur;
                data.Keterangan = vm.Keterangan;

                data.UpdateDateTime = DateTime.UtcNow;
                data.UpdateBy = userActiveId.Value;

                _applicationDbContext.HeaderReturs.Update(data);

                await _applicationDbContext.SaveChangesAsync();

                await RecalculateStatusReturAsync(id, userActiveId.Value);

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
        // DELETE HEADER + ITEM
        // =====================================================

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            using var transaction =
                await _applicationDbContext.Database.BeginTransactionAsync();

            try
            {
                var data =
                    await _applicationDbContext.HeaderReturs
                    .FirstOrDefaultAsync(x =>
                        x.HeaderReturId == id &&
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

                _applicationDbContext.HeaderReturs.Update(data);

                var items =
                    await _applicationDbContext.ItemReturs
                    .Where(x =>
                        x.HeaderReturId == id &&
                        x.IsDelete == false)
                    .ToListAsync();

                foreach (var item in items)
                {
                    item.IsDelete = true;
                    item.DeleteDateTime = DateTime.UtcNow;
                    item.DeleteBy = userActiveId.Value;

                    _applicationDbContext.ItemReturs.Update(item);
                }

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