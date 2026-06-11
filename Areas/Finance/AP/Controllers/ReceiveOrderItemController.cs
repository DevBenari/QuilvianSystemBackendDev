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
    public class ReceiveOrderItemController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<ReceiveOrderItemController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ReceiveOrderItemController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ReceiveOrderItemController> logger,
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
        public async Task<IActionResult> PagedReceiveOrderItem(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "ProductName",
            string? sortDirection = "asc",
            Guid? receiveOrderId = null,
            Guid? productId = null,

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
                    from item in _applicationDbContext.ReceiveOrderItems.AsNoTracking()

                    join ro in _applicationDbContext.ReceiveOrders.AsNoTracking()
                        on item.ReceiveOrderId equals ro.ReceiveOrderId

                    where item.IsDelete == false &&
                          ro.IsDelete == false

                    select new
                    {
                        item.ReceiveOrderItemId,
                        item.ReceiveOrderId,
                        item.ProductId,
                        item.Barcode,
                        item.ProductName,
                        item.Measure,
                        item.Category,
                        item.Remarks,
                        item.QtyOrder,
                        item.QtyReceive,
                        item.StampDuty,
                        item.ExpiredDate,
                        item.BatchNumber,
                        item.Keterangan,
                        item.CreateDateTime,

                        ro.ReceiveOrderNumber,
                        ro.InvoiceNumber,
                        ro.DeliveryNumber,
                        ro.SupplierId,
                        ro.Status
                    };

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = $"%{search.Trim().ToLower()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.ReceiveOrderNumber ?? "", search) ||
                        EF.Functions.ILike(x.InvoiceNumber ?? "", search) ||
                        EF.Functions.ILike(x.DeliveryNumber ?? "", search) ||
                        EF.Functions.ILike(x.Barcode ?? "", search) ||
                        EF.Functions.ILike(x.ProductName ?? "", search) ||
                        EF.Functions.ILike(x.Measure ?? "", search) ||
                        EF.Functions.ILike(x.Category ?? "", search) ||
                        EF.Functions.ILike(x.BatchNumber ?? "", search) ||
                        EF.Functions.ILike(x.Keterangan ?? "", search)
                    );
                }

                if (receiveOrderId.HasValue)
                {
                    query = query.Where(x =>
                        x.ReceiveOrderId == receiveOrderId.Value);
                }

                if (productId.HasValue)
                {
                    query = query.Where(x =>
                        x.ProductId == productId.Value);
                }

                // FILTER DATE BERDASARKAN EXPIRED DATE
                if (startDate.HasValue && endDate.HasValue)
                {
                    DateTime startUtc = startDate.Value.Date.ToUniversalTime();

                    DateTime endUtc = endDate.Value.Date
                        .AddDays(1)
                        .AddTicks(-1)
                        .ToUniversalTime();

                    query = query.Where(x =>
                        x.ExpiredDate >= startUtc &&
                        x.ExpiredDate <= endUtc);
                }

                var sortColumn = orderBy?.ToLower() ?? "productname";
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

                    "barcode" =>
                        isDescending
                            ? query.OrderByDescending(x => x.Barcode)
                            : query.OrderBy(x => x.Barcode),

                    "productname" =>
                        isDescending
                            ? query.OrderByDescending(x => x.ProductName)
                            : query.OrderBy(x => x.ProductName),

                    "category" =>
                        isDescending
                            ? query.OrderByDescending(x => x.Category)
                            : query.OrderBy(x => x.Category),

                    "qtyorder" =>
                        isDescending
                            ? query.OrderByDescending(x => x.QtyOrder)
                            : query.OrderBy(x => x.QtyOrder),

                    "qtyreceive" =>
                        isDescending
                            ? query.OrderByDescending(x => x.QtyReceive)
                            : query.OrderBy(x => x.QtyReceive),

                    "expireddate" =>
                        isDescending
                            ? query.OrderByDescending(x => x.ExpiredDate)
                            : query.OrderBy(x => x.ExpiredDate),

                    "batchnumber" =>
                        isDescending
                            ? query.OrderByDescending(x => x.BatchNumber)
                            : query.OrderBy(x => x.BatchNumber),

                    "createdatetime" =>
                        isDescending
                            ? query.OrderByDescending(x => x.CreateDateTime)
                            : query.OrderBy(x => x.CreateDateTime),

                    _ =>
                        query.OrderBy(x => x.ProductName)
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
                var data = await _applicationDbContext.ReceiveOrderItems
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.ReceiveOrderItemId == id &&
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
            [FromBody] ReceiveOrderItemViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                bool receiveOrderExists =
                    await _applicationDbContext.ReceiveOrders
                    .AnyAsync(x =>
                        x.ReceiveOrderId == vm.ReceiveOrderId &&
                        x.IsDelete == false);

                if (!receiveOrderExists)
                {
                    return NotFound(new
                    {
                        message = "Receive Order tidak ditemukan."
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

                var data = new ReceiveOrderItem
                {
                    ReceiveOrderItemId = Guid.NewGuid(),
                    ReceiveOrderId = vm.ReceiveOrderId,
                    ProductId = vm.ProductId,
                    Barcode = vm.Barcode,
                    ProductName = vm.ProductName,
                    Measure = vm.Measure,
                    Category = vm.Category,
                    Remarks = vm.Remarks,
                    QtyOrder = vm.QtyOrder,
                    QtyReceive = vm.QtyReceive,
                    StampDuty = vm.StampDuty,
                    ExpiredDate = vm.ExpiredDate,
                    BatchNumber = vm.BatchNumber,
                    Keterangan = vm.Keterangan,

                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = userActiveId.Value,
                    IsDelete = false
                };

                _applicationDbContext.ReceiveOrderItems.Add(data);

                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Created("", new
                    {
                        message = "Tambah data berhasil.",
                        data = new
                        {
                            data.ReceiveOrderItemId
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
            [FromBody] ReceiveOrderItemViewModel vm)
        {
            try
            {
                var data = await _applicationDbContext.ReceiveOrderItems
                    .FirstOrDefaultAsync(x =>
                        x.ReceiveOrderItemId == id &&
                        x.IsDelete == false);

                if (data == null)
                {
                    return NotFound(new
                    {
                        message = "Data tidak ditemukan."
                    });
                }

                bool receiveOrderExists =
                    await _applicationDbContext.ReceiveOrders
                    .AnyAsync(x =>
                        x.ReceiveOrderId == vm.ReceiveOrderId &&
                        x.IsDelete == false);

                if (!receiveOrderExists)
                {
                    return NotFound(new
                    {
                        message = "Receive Order tidak ditemukan."
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

                data.ReceiveOrderId = vm.ReceiveOrderId;
                data.ProductId = vm.ProductId;
                data.Barcode = vm.Barcode;
                data.ProductName = vm.ProductName;
                data.Measure = vm.Measure;
                data.Category = vm.Category;
                data.Remarks = vm.Remarks;
                data.QtyOrder = vm.QtyOrder;
                data.QtyReceive = vm.QtyReceive;
                data.StampDuty = vm.StampDuty;
                data.ExpiredDate = vm.ExpiredDate;
                data.BatchNumber = vm.BatchNumber;
                data.Keterangan = vm.Keterangan;

                data.UpdateDateTime = DateTime.UtcNow;
                data.UpdateBy = userActiveId.Value;

                _applicationDbContext.ReceiveOrderItems.Update(data);

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
                var data = await _applicationDbContext.ReceiveOrderItems
                    .FirstOrDefaultAsync(x =>
                        x.ReceiveOrderItemId == id &&
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

                _applicationDbContext.ReceiveOrderItems.Update(data);

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