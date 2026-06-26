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
    public class DepositReturController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<DepositReturController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DepositReturController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<DepositReturController> logger,
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
        // PAGED DEPOSIT RETUR
        // =====================================================

        [HttpGet("paged")]
        public async Task<IActionResult> PagedDepositRetur(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "TglInsertDeposit",
            string? sortDirection = "desc",

            Guid? supplierId = null,
            Guid? poId = null,
            Guid? receiveOrderId = null,
            Guid? headerReturId = null,
            string? statusDeposit = null,

            decimal? minJumlahDeposit = null,
            decimal? maxJumlahDeposit = null,

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
                    _applicationDbContext.DepositReturs
                    .AsNoTracking()
                    .Where(x => x.IsDelete == false);

                // =========================
                // Search
                // =========================
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var keyword = $"%{search.Trim()}%";

                    baseQuery = baseQuery.Where(x =>
                        EF.Functions.ILike(x.StatusDeposit ?? "", keyword) ||
                        EF.Functions.ILike(x.Keterangan ?? "", keyword) ||

                        _applicationDbContext.Suppliers.Any(s =>
                            s.SupplierId == x.SupplierId &&
                            EF.Functions.ILike(s.SupplierName ?? "", keyword)
                        ) ||

                        _applicationDbContext.PurchaseOrders.Any(po =>
                            po.PurchaseOrderId == x.PoId &&
                            EF.Functions.ILike(po.PurchaseOrderNumber ?? "", keyword)
                        ) ||

                        _applicationDbContext.ReceiveOrders.Any(ro =>
                            ro.ReceiveOrderId == x.ReceiveOrderId &&
                            EF.Functions.ILike(ro.ReceiveOrderNumber ?? "", keyword)
                        ) ||

                        _applicationDbContext.HeaderReturs.Any(hr =>
                            hr.HeaderReturId == x.HeaderReturId &&
                            EF.Functions.ILike(hr.KodeRetur ?? "", keyword)
                        )
                    );
                }

                // =========================
                // Filter Supplier
                // =========================
                if (supplierId.HasValue)
                {
                    baseQuery = baseQuery.Where(x =>
                        x.SupplierId == supplierId.Value);
                }

                // =========================
                // Filter PO
                // =========================
                if (poId.HasValue)
                {
                    baseQuery = baseQuery.Where(x =>
                        x.PoId == poId.Value);
                }

                // =========================
                // Filter Receive Order
                // =========================
                if (receiveOrderId.HasValue)
                {
                    baseQuery = baseQuery.Where(x =>
                        x.ReceiveOrderId == receiveOrderId.Value);
                }

                // =========================
                // Filter Retur
                // =========================
                if (headerReturId.HasValue)
                {
                    baseQuery = baseQuery.Where(x =>
                        x.HeaderReturId == headerReturId.Value);
                }

                // =========================
                // Filter Status Deposit
                // =========================
                if (!string.IsNullOrWhiteSpace(statusDeposit))
                {
                    var status = statusDeposit.Trim();

                    baseQuery = baseQuery.Where(x =>
                        x.StatusDeposit != null &&
                        x.StatusDeposit.ToLower() == status.ToLower());
                }

                // =========================
                // Filter Jumlah Deposit
                // =========================
                if (minJumlahDeposit.HasValue)
                {
                    baseQuery = baseQuery.Where(x =>
                        x.JumlahDeposit >= minJumlahDeposit.Value);
                }

                if (maxJumlahDeposit.HasValue)
                {
                    baseQuery = baseQuery.Where(x =>
                        x.JumlahDeposit <= maxJumlahDeposit.Value);
                }

                // =========================
                // Filter Tanggal
                // =========================
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
                        x.TglInsertDeposit >= startUtc &&
                        x.TglInsertDeposit <= endUtc);
                }

                // =========================
                // Select Data
                // =========================
                var query =
                    baseQuery.Select(x => new
                    {
                        x.DepositReturId,

                        x.PoId,
                        NoPO =
                            _applicationDbContext.PurchaseOrders
                            .Where(po => po.PurchaseOrderId == x.PoId)
                            .Select(po => po.PurchaseOrderNumber)
                            .FirstOrDefault(),

                        x.SupplierId,

                        Supplier =
                            _applicationDbContext.Suppliers
                            .Where(s => s.SupplierId == x.SupplierId)
                            .Select(s => new
                            {
                                s.SupplierId,
                                s.SupplierCode,
                                s.SupplierName,
                                s.ContactPerson,
                                s.TermOfPayment,
                                s.LeadTime,
                                s.Address,
                                s.City,
                                s.PhoneNumber,
                                s.Email,
                                s.IsPKS,
                                s.IsActive,
                                s.BankId,
                                s.NoRekening,
                                s.AccountHolderName,
                                s.IsFullPaid,
                                s.IsBloodBankSupplier,
                                s.PaymentMethod,
                                s.PPN,
                                s.Note
                            })
                            .FirstOrDefault(),

                        NamaSupplier =
                            _applicationDbContext.Suppliers
                            .Where(s => s.SupplierId == x.SupplierId)
                            .Select(s => s.SupplierName)
                            .FirstOrDefault(),

                        x.ReceiveOrderId,
                        ReceiveOrderNumber =
                            _applicationDbContext.ReceiveOrders
                            .Where(ro => ro.ReceiveOrderId == x.ReceiveOrderId)
                            .Select(ro => ro.ReceiveOrderNumber)
                            .FirstOrDefault(),

                        x.HeaderReturId,
                        KodeRetur =
                            _applicationDbContext.HeaderReturs
                            .Where(hr => hr.HeaderReturId == x.HeaderReturId)
                            .Select(hr => hr.KodeRetur)
                            .FirstOrDefault(),

                        x.TglInsertDeposit,
                        x.StatusDeposit,
                        x.JumlahDeposit,
                        x.Keterangan,

                        x.CreateDateTime,
                        x.UpdateDateTime
                    });

                // =========================
                // Sorting
                // =========================
                var sortColumn =
                    orderBy?.ToLower() ?? "tglinsertdeposit";

                var isDescending =
                    sortDirection?.ToLower() == "desc";

                query = sortColumn switch
                {
                    "nopo" =>
                        isDescending
                            ? query.OrderByDescending(x => x.NoPO)
                            : query.OrderBy(x => x.NoPO),

                    "namasupplier" =>
                        isDescending
                            ? query.OrderByDescending(x => x.NamaSupplier)
                            : query.OrderBy(x => x.NamaSupplier),

                    "receiveordernumber" =>
                        isDescending
                            ? query.OrderByDescending(x => x.ReceiveOrderNumber)
                            : query.OrderBy(x => x.ReceiveOrderNumber),

                    "koderetur" =>
                        isDescending
                            ? query.OrderByDescending(x => x.KodeRetur)
                            : query.OrderBy(x => x.KodeRetur),

                    "tglinsertdeposit" =>
                        isDescending
                            ? query.OrderByDescending(x => x.TglInsertDeposit)
                            : query.OrderBy(x => x.TglInsertDeposit),

                    "statusdeposit" =>
                        isDescending
                            ? query.OrderByDescending(x => x.StatusDeposit)
                            : query.OrderBy(x => x.StatusDeposit),

                    "jumlahdeposit" =>
                        isDescending
                            ? query.OrderByDescending(x => x.JumlahDeposit)
                            : query.OrderBy(x => x.JumlahDeposit),

                    _ =>
                        query.OrderByDescending(x => x.TglInsertDeposit)
                };

                // =========================
                // Pagination
                // =========================
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
                    await _applicationDbContext.DepositReturs
                    .AsNoTracking()
                    .Where(x =>
                        x.DepositReturId == id &&
                        x.IsDelete == false)
                    .Select(x => new
                    {
                        x.DepositReturId,

                        x.PoId,
                        NoPO =
                            _applicationDbContext.PurchaseOrders
                            .Where(po => po.PurchaseOrderId == x.PoId)
                            .Select(po => po.PurchaseOrderNumber)
                            .FirstOrDefault(),

                        x.SupplierId,

                        Supplier =
                            _applicationDbContext.Suppliers
                            .Where(s => s.SupplierId == x.SupplierId)
                            .Select(s => new
                            {
                                s.SupplierId,
                                s.SupplierCode,
                                s.SupplierName,
                                s.ContactPerson,
                                s.TermOfPayment,
                                s.LeadTime,
                                s.Address,
                                s.City,
                                s.PhoneNumber,
                                s.Email,
                                s.IsPKS,
                                s.IsActive,
                                s.BankId,
                                s.NoRekening,
                                s.AccountHolderName,
                                s.IsFullPaid,
                                s.IsBloodBankSupplier,
                                s.PaymentMethod,
                                s.PPN,
                                s.Note
                            })
                            .FirstOrDefault(),

                        NamaSupplier =
                            _applicationDbContext.Suppliers
                            .Where(s => s.SupplierId == x.SupplierId)
                            .Select(s => s.SupplierName)
                            .FirstOrDefault(),

                        x.ReceiveOrderId,
                        ReceiveOrderNumber =
                            _applicationDbContext.ReceiveOrders
                            .Where(ro => ro.ReceiveOrderId == x.ReceiveOrderId)
                            .Select(ro => ro.ReceiveOrderNumber)
                            .FirstOrDefault(),

                        x.HeaderReturId,
                        KodeRetur =
                            _applicationDbContext.HeaderReturs
                            .Where(hr => hr.HeaderReturId == x.HeaderReturId)
                            .Select(hr => hr.KodeRetur)
                            .FirstOrDefault(),

                        x.TglInsertDeposit,
                        x.StatusDeposit,
                        x.JumlahDeposit,
                        x.Keterangan,

                        x.CreateDateTime,
                        x.CreateBy,
                        x.UpdateDateTime,
                        x.UpdateBy
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
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] DepositReturViewModel vm)
        {
            using var transaction =
                await _applicationDbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

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

                var supplier =
                    await _applicationDbContext.Suppliers
                    .AsNoTracking()
                    .Where(x =>
                        x.SupplierId == vm.SupplierId &&
                        x.IsDelete == false)
                    .Select(x => new
                    {
                        x.SupplierId,
                        x.SupplierCode,
                        x.SupplierName,
                        x.ContactPerson,
                        x.TermOfPayment,
                        x.LeadTime,
                        x.Address,
                        x.City,
                        x.PhoneNumber,
                        x.Email,
                        x.IsPKS,
                        x.IsActive,
                        x.BankId,
                        x.NoRekening,
                        x.AccountHolderName,
                        x.IsFullPaid,
                        x.IsBloodBankSupplier,
                        x.PaymentMethod,
                        x.PPN,
                        x.Note
                    })
                    .FirstOrDefaultAsync();

                if (supplier == null)
                {
                    return BadRequest(new
                    {
                        message = "Supplier tidak ditemukan."
                    });
                }

                var poExists =
                    await _applicationDbContext.PurchaseOrders
                    .AnyAsync(x =>
                        x.PurchaseOrderId == vm.PoId &&
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

                var headerReturExists =
                    await _applicationDbContext.HeaderReturs
                    .AnyAsync(x =>
                        x.HeaderReturId == vm.HeaderReturId &&
                        x.IsDelete == false);

                if (!headerReturExists)
                {
                    return BadRequest(new
                    {
                        message = "Data retur tidak ditemukan."
                    });
                }

                var depositReturId =
                    Guid.NewGuid();

                var data = new DepositRetur
                {
                    DepositReturId = depositReturId,

                    PoId = vm.PoId,
                    SupplierId = vm.SupplierId,
                    ReceiveOrderId = vm.ReceiveOrderId,
                    HeaderReturId = vm.HeaderReturId,

                    TglInsertDeposit =
                        vm.TglInsertDeposit == default
                            ? DateTime.UtcNow
                            : vm.TglInsertDeposit,

                    StatusDeposit =
                        string.IsNullOrWhiteSpace(vm.StatusDeposit)
                            ? "Menunggu"
                            : vm.StatusDeposit,

                    JumlahDeposit = vm.JumlahDeposit,
                    Keterangan = vm.Keterangan,

                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = userActiveId.Value,
                    IsDelete = false
                };

                _applicationDbContext.DepositReturs.Add(data);

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
                            DepositReturId = depositReturId,
                            Supplier = supplier
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
        // UPDATE
        // =====================================================

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] DepositReturViewModel vm)
        {
            using var transaction =
                await _applicationDbContext.Database.BeginTransactionAsync();

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var data =
                    await _applicationDbContext.DepositReturs
                    .FirstOrDefaultAsync(x =>
                        x.DepositReturId == id &&
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

                var supplierExists =
                    await _applicationDbContext.Suppliers
                    .AnyAsync(x =>
                        x.SupplierId == vm.SupplierId &&
                        x.IsDelete == false);

                if (!supplierExists)
                {
                    return BadRequest(new
                    {
                        message = "Supplier tidak ditemukan."
                    });
                }

                var poExists =
                    await _applicationDbContext.PurchaseOrders
                    .AnyAsync(x =>
                        x.PurchaseOrderId == vm.PoId &&
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

                var headerReturExists =
                    await _applicationDbContext.HeaderReturs
                    .AnyAsync(x =>
                        x.HeaderReturId == vm.HeaderReturId &&
                        x.IsDelete == false);

                if (!headerReturExists)
                {
                    return BadRequest(new
                    {
                        message = "Data retur tidak ditemukan."
                    });
                }

                data.PoId = vm.PoId;
                data.SupplierId = vm.SupplierId;
                data.ReceiveOrderId = vm.ReceiveOrderId;
                data.HeaderReturId = vm.HeaderReturId;

                data.TglInsertDeposit = vm.TglInsertDeposit;
                data.StatusDeposit = vm.StatusDeposit;
                data.JumlahDeposit = vm.JumlahDeposit;
                data.Keterangan = vm.Keterangan;

                data.UpdateDateTime = DateTime.UtcNow;
                data.UpdateBy = userActiveId.Value;

                _applicationDbContext.DepositReturs.Update(data);

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
                    await _applicationDbContext.DepositReturs
                    .FirstOrDefaultAsync(x =>
                        x.DepositReturId == id &&
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

                _applicationDbContext.DepositReturs.Update(data);

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