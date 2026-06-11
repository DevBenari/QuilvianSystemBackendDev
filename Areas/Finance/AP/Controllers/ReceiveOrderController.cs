using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.Finance.AP.Models;
using QuilvianSystemBackendDev.Areas.Finance.AP.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace QuilvianSystemBackendDev.Areas.Finance.AP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class ReceiveOrderController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<ReceiveOrderController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ReceiveOrderController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ReceiveOrderController> logger,
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
            var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(emailLogin))
                return null;

            var userActive = await _applicationDbContext.UserActives
                .FirstOrDefaultAsync(x => x.Email == emailLogin);

            return userActive?.UserActiveId;
        }

        // =====================================================
        // PAGED
        // =====================================================

        [HttpGet("paged")]
        public async Task<IActionResult> PagedReceiveOrder(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            Guid? supplierId = null,
            Guid? purchaseOrderId = null,
            string? status = null,
            bool? isInvoiceProvided = null,

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

                var query =
                    from ro in _applicationDbContext.ReceiveOrders.AsNoTracking()

                    join u in _applicationDbContext.UserActives.AsNoTracking()
                        on ro.CreateBy equals u.UserActiveId into userJoin

                    from u in userJoin.DefaultIfEmpty()

                    where ro.IsDelete == false

                    select new
                    {
                        ro.ReceiveOrderId,
                        ro.ReceiveOrderNumber,
                        ro.PurchaseOrderId,
                        ro.InvoiceNumber,
                        ro.IsInvoiceProvided,
                        ro.DeliveryNumber,
                        ro.DueDate,
                        ro.TermOfPayment,
                        ro.SupplierId,
                        ro.StampDuty,
                        ro.AdditionalDiscountRp,
                        ro.Status,
                        ro.Keterangan,
                        ro.CreateDateTime,

                        CreateByName = u != null ? u.FullName : null,

                        TotalItem =
                            _applicationDbContext.ReceiveOrderItems
                            .Count(i =>
                                i.ReceiveOrderId == ro.ReceiveOrderId &&
                                i.IsDelete == false)
                    };

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = $"%{search.Trim().ToLower()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.ReceiveOrderNumber ?? "", search) ||
                        EF.Functions.ILike(x.InvoiceNumber ?? "", search) ||
                        EF.Functions.ILike(x.DeliveryNumber ?? "", search) ||
                        EF.Functions.ILike(x.Status ?? "", search) ||
                        EF.Functions.ILike(x.Keterangan ?? "", search)
                    );
                }

                if (supplierId.HasValue)
                {
                    query = query.Where(x => x.SupplierId == supplierId.Value);
                }

                if (purchaseOrderId.HasValue)
                {
                    query = query.Where(x => x.PurchaseOrderId == purchaseOrderId.Value);
                }

                if (!string.IsNullOrWhiteSpace(status))
                {
                    query = query.Where(x =>
                        x.Status != null &&
                        x.Status.ToLower() == status.ToLower());
                }

                if (isInvoiceProvided.HasValue)
                {
                    query = query.Where(x =>
                        x.IsInvoiceProvided == isInvoiceProvided.Value);
                }

                // FILTER DATE BERDASARKAN DUE DATE
                if (startDate.HasValue && endDate.HasValue)
                {
                    DateTime startUtc = startDate.Value.Date.ToUniversalTime();

                    DateTime endUtc = endDate.Value.Date
                        .AddDays(1)
                        .AddTicks(-1)
                        .ToUniversalTime();

                    query = query.Where(x =>
                        x.DueDate >= startUtc &&
                        x.DueDate <= endUtc);
                }

                var sortColumn = orderBy?.ToLower() ?? "createdatetime";
                var isDescending = sortDirection?.ToLower() == "desc";

                query = sortColumn switch
                {
                    "receiveordernumber" =>
                        isDescending
                            ? query.OrderByDescending(x => x.ReceiveOrderNumber)
                            : query.OrderBy(x => x.ReceiveOrderNumber),

                    "invoicenumber" =>
                        isDescending
                            ? query.OrderByDescending(x => x.InvoiceNumber)
                            : query.OrderBy(x => x.InvoiceNumber),

                    "deliverynumber" =>
                        isDescending
                            ? query.OrderByDescending(x => x.DeliveryNumber)
                            : query.OrderBy(x => x.DeliveryNumber),

                    "duedate" =>
                        isDescending
                            ? query.OrderByDescending(x => x.DueDate)
                            : query.OrderBy(x => x.DueDate),

                    "termofpayment" =>
                        isDescending
                            ? query.OrderByDescending(x => x.TermOfPayment)
                            : query.OrderBy(x => x.TermOfPayment),

                    "status" =>
                        isDescending
                            ? query.OrderByDescending(x => x.Status)
                            : query.OrderBy(x => x.Status),

                    "createdatetime" =>
                        isDescending
                            ? query.OrderByDescending(x => x.CreateDateTime)
                            : query.OrderBy(x => x.CreateDateTime),

                    _ =>
                        query.OrderByDescending(x => x.CreateDateTime)
                };

                int totalRows = await query.CountAsync();

                int totalPages =
                    (int)Math.Ceiling(totalRows / (double)perPage);

                var rows = await query
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
                var data = await _applicationDbContext.ReceiveOrders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.ReceiveOrderId == id &&
                        x.IsDelete == false);

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
            [FromBody] ReceiveOrderViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userActiveId = await GetUserActiveId();

                if (userActiveId == null)
                {
                    return Unauthorized(new
                    {
                        message = "User aktif tidak ditemukan."
                    });
                }

                var data = new ReceiveOrder
                {
                    ReceiveOrderId = Guid.NewGuid(),
                    ReceiveOrderNumber = vm.ReceiveOrderNumber,
                    PurchaseOrderId = vm.PurchaseOrderId,
                    InvoiceNumber = vm.InvoiceNumber,
                    IsInvoiceProvided = vm.IsInvoiceProvided,
                    DeliveryNumber = vm.DeliveryNumber,
                    DueDate = vm.DueDate,
                    TermOfPayment = vm.TermOfPayment,
                    SupplierId = vm.SupplierId,
                    StampDuty = vm.StampDuty,
                    AdditionalDiscountRp = vm.AdditionalDiscountRp,
                    Status = vm.Status,
                    Keterangan = vm.Keterangan,

                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = userActiveId.Value,
                    IsDelete = false
                };

                _applicationDbContext.ReceiveOrders.Add(data);

                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Created("", new
                    {
                        message = "Tambah data berhasil.",
                        data = new
                        {
                            data.ReceiveOrderId
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
            [FromBody] ReceiveOrderViewModel vm)
        {
            try
            {
                var data = await _applicationDbContext.ReceiveOrders
                    .FirstOrDefaultAsync(x =>
                        x.ReceiveOrderId == id &&
                        x.IsDelete == false);

                if (data == null)
                {
                    return NotFound(new
                    {
                        message = "Data tidak ditemukan."
                    });
                }

                var userActiveId = await GetUserActiveId();

                if (userActiveId == null)
                {
                    return Unauthorized(new
                    {
                        message = "User aktif tidak ditemukan."
                    });
                }

                data.ReceiveOrderNumber = vm.ReceiveOrderNumber;
                data.PurchaseOrderId = vm.PurchaseOrderId;
                data.InvoiceNumber = vm.InvoiceNumber;
                data.IsInvoiceProvided = vm.IsInvoiceProvided;
                data.DeliveryNumber = vm.DeliveryNumber;
                data.DueDate = vm.DueDate;
                data.TermOfPayment = vm.TermOfPayment;
                data.SupplierId = vm.SupplierId;
                data.StampDuty = vm.StampDuty;
                data.AdditionalDiscountRp = vm.AdditionalDiscountRp;
                data.Status = vm.Status;
                data.Keterangan = vm.Keterangan;

                data.UpdateDateTime = DateTime.UtcNow;
                data.UpdateBy = userActiveId.Value;

                _applicationDbContext.ReceiveOrders.Update(data);

                int result = await _applicationDbContext.SaveChangesAsync();

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
            try
            {
                var data = await _applicationDbContext.ReceiveOrders
                    .FirstOrDefaultAsync(x =>
                        x.ReceiveOrderId == id &&
                        x.IsDelete == false);

                if (data == null)
                {
                    return NotFound(new
                    {
                        message = "Data tidak ditemukan."
                    });
                }

                var userActiveId = await GetUserActiveId();

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

                _applicationDbContext.ReceiveOrders.Update(data);

                int result = await _applicationDbContext.SaveChangesAsync();

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
                _logger.LogError(ex, ex.Message);

                return StatusCode(500, new
                {
                    message = ex.Message
                });
            }
        }
    }
}