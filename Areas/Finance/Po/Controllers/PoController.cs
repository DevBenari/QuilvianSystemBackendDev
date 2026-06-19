using System;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.Finance.Po.Models;
using QuilvianSystemBackendDev.Areas.Finance.Po.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.Finance.Po.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class PoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PoController(ApplicationDbContext context)
        {
            _context = context;
        }
        // =========================
        // POST: api/purchaseorders
        // =========================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PurchaseOrderViewModel dto)
        {
            if (dto == null)
                return BadRequest("Data tidak valid");
            if (dto.Items == null || !dto.Items.Any())
                return BadRequest("Detail item tidak boleh kosong");

            var po = new PurchaseOrder
            {
                PurchaseOrderId = Guid.NewGuid(),

                PurchaseRequestNumber = dto.PurchaseRequestNumber,
                PurchaseOrderNumber = dto.PurchaseOrderNumber,
                InvoiceDate = dto.InvoiceDate,
                InvoiceNumber = dto.InvoiceNumber,
                RequestType = dto.RequestType,

                SupplierId = dto.SupplierId,
                SupplierCode = dto.SupplierCode,
                SupplierName = dto.SupplierName,

                TermOfPayment = dto.TermOfPayment,
                ExpiredDate = dto.ExpiredDate,

                RemainingDay = dto.RemainingDay,
                QtyTotal = dto.QtyTotal,
                GrandTotal = dto.GrandTotal,

                UserAccess = dto.UserAccess,
                StatusPO = dto.StatusPO,
                Keterangan = dto.Keterangan,

                PurchaseOrderItems = dto.Items.Select(i => new PurchaseOrderItem
                {
                    PurchaseOrderItemId = Guid.NewGuid(),
                    ProductName = i.ProductName,
                    Measurement = i.Measurement,
                    Category = i.Category,
                    Qty = i.Qty ?? 0,
                    Price = i.Price ?? 0,
                    Discount = i.Discount ?? 0,
                    SubTotal = i.SubTotal ?? 0,
                    Keterangan = i.Keterangan
                }).ToList()
            };

            _context.PurchaseOrders.Add(po);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Purchase Order berhasil disimpan",
                purchaseOrderId = po.PurchaseOrderId
            });

        }

        //[HttpPost]
        //public async Task<IActionResult> Create(PurchaseOrderViewModel dto)
        //{
        //    // ===============================
        //    // 1. Cek / Insert Supplier
        //    // ===============================
        //    var supplier = await _context.Suppliers
        //        .FirstOrDefaultAsync(x => x.SupplierCode == dto.SupplierCode);

        //    if (supplier == null)
        //    {
        //        supplier = new Supplier
        //        {
        //            SupplierId = Guid.NewGuid(),
        //            SupplierCode = dto.SupplierCode!,
        //            SupplierName = dto.SupplierName!,
        //            IsActive = true
        //        };

        //        _context.Suppliers.Add(supplier);
        //        await _context.SaveChangesAsync(); // simpan supplier dulu
        //    }

        //    // ===============================
        //    // 2. Insert Purchase Order
        //    // ===============================
        //    var po = new PurchaseOrder
        //    {
        //        PurchaseOrderId = Guid.NewGuid(),

        //        SupplierCode = supplier.SupplierCode,
        //        SupplierName = supplier.SupplierName,

        //        PurchaseRequestNumber = dto.PurchaseRequestNumber,
        //        PurchaseOrderNumber = dto.PurchaseOrderNumber,
        //        InvoiceDate = dto.InvoiceDate,
        //        InvoiceNumber = dto.InvoiceNumber,
        //        RequestType = dto.RequestType,

        //        TermOfPayment = dto.TermOfPayment,
        //        ExpiredDate = dto.ExpiredDate,

        //        RemainingDay = dto.RemainingDay,
        //        QtyTotal = dto.QtyTotal,
        //        GrandTotal = dto.GrandTotal,

        //        UserAccess = dto.UserAccess,
        //        StatusPO = dto.StatusPO,
        //        Keterangan = dto.Keterangan,

        //        PurchaseOrderItems = dto.Items.Select(i => new PurchaseOrderItem
        //        {
        //            PurchaseOrderItemId = Guid.NewGuid(),
        //            ProductName = i.ProductName,
        //            Measurement = i.Measurement,
        //            Category = i.Category,
        //            Qty = i.Qty ?? 0,
        //            Price = i.Price ?? 0,
        //            Discount = i.Discount ?? 0,
        //            SubTotal = i.SubTotal ?? 0,
        //            Keterangan = i.Keterangan
        //        }).ToList()
        //    };

        //    _context.PurchaseOrders.Add(po);
        //    await _context.SaveChangesAsync();

        //    return Ok(new
        //    {
        //        message = "Purchase Order & Supplier berhasil disimpan",
        //        purchaseOrderId = po.PurchaseOrderId,
        //        supplierId = supplier.SupplierId
        //    });
        //}


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.PurchaseOrders
                .Include(x => x.PurchaseOrderItems)
                .OrderByDescending(x => x.PurchaseOrderNumber)
                .Select(po => new PurchaseOrderViewModel
                {
                    PurchaseRequestNumber = po.PurchaseRequestNumber,
                    PurchaseOrderNumber = po.PurchaseOrderNumber,
                    InvoiceDate = po.InvoiceDate,
                    InvoiceNumber = po.InvoiceNumber,
                    RequestType = po.RequestType,

                    SupplierId = po.SupplierId,
                    SupplierName = po.SupplierName,
                    SupplierCode = po.SupplierCode,

                    TermOfPayment = po.TermOfPayment,
                    ExpiredDate = po.ExpiredDate,

                    RemainingDay = po.RemainingDay,
                    QtyTotal = po.QtyTotal,
                    GrandTotal = po.GrandTotal,

                    UserAccess = po.UserAccess,
                    StatusPO = po.StatusPO,
                    Keterangan = po.Keterangan,

                    Items = po.PurchaseOrderItems.Select(i => new PurchaseOrderItemViewModel
                    {
                        ProductName = i.ProductName,
                        Measurement = i.Measurement,
                        Category = i.Category,
                        Qty = i.Qty,
                        Price = i.Price,
                        Discount = i.Discount,
                        SubTotal = i.SubTotal,
                        Keterangan = i.Keterangan
                    }).ToList()
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(string? search = null)
        {
            var query = _context.PurchaseOrders
                .Include(x => x.PurchaseOrderItems)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.SupplierName.Contains(search) ||
                    x.SupplierCode.Contains(search));
            }

            var data = await query
                .OrderByDescending(x => x.PurchaseOrderNumber)
                .GroupBy(x => new
                {
                    x.SupplierCode,
                    x.SupplierName
                })
                .Select(g => new
                {
                    SupplierCode = g.Key.SupplierCode,
                    SupplierName = g.Key.SupplierName,
                    TotalPO = g.Count(),
                    PurchaseOrders = g.Select(po => new PurchaseOrderViewModel
                    {
                        PurchaseRequestNumber = po.PurchaseRequestNumber,
                        PurchaseOrderNumber = po.PurchaseOrderNumber,
                        SupplierId = po.SupplierId,
                        InvoiceDate = po.InvoiceDate,
                        InvoiceNumber = po.InvoiceNumber,
                        RequestType = po.RequestType,

                        TermOfPayment = po.TermOfPayment,
                        ExpiredDate = po.ExpiredDate,
                        RemainingDay = po.RemainingDay,
                        QtyTotal = po.QtyTotal,
                        GrandTotal = po.GrandTotal,
                        UserAccess = po.UserAccess,
                        StatusPO = po.StatusPO,
                        Keterangan = po.Keterangan,

                        Items = po.PurchaseOrderItems.Select(i => new PurchaseOrderItemViewModel
                        {
                            ProductName = i.ProductName,
                            Measurement = i.Measurement,
                            Category = i.Category,
                            Qty = i.Qty,
                            Price = i.Price,
                            Discount = i.Discount,
                            SubTotal = i.SubTotal,
                            Keterangan = i.Keterangan
                        }).ToList()
                    }).ToList()
                })
                .ToListAsync();

            return Ok(new { message = "Success", data });
        }

        [HttpGet("by-purchase-order/{purchaseOrderId:guid}")]
        public async Task<IActionResult> GetByPurchaseOrderId(Guid purchaseOrderId)
        {
            var data = await _context.PurchaseOrders
                .AsNoTracking()
                .Where(po =>
                    po.PurchaseOrderId == purchaseOrderId &&
                    (po.IsDelete == false || po.IsDelete == null))
                .Select(po => new
                {
                    PurchaseOrderId = po.PurchaseOrderId,
                    PurchaseRequestNumber = po.PurchaseRequestNumber,
                    PurchaseOrderNumber = po.PurchaseOrderNumber,
                    InvoiceDate = po.InvoiceDate,
                    InvoiceNumber = po.InvoiceNumber,
                    RequestType = po.RequestType,

                    SupplierId = po.SupplierId,
                    SupplierName = po.SupplierName,
                    SupplierCode = po.SupplierCode,

                    TermOfPayment = po.TermOfPayment,
                    ExpiredDate = po.ExpiredDate,

                    RemainingDay = po.RemainingDay,
                    QtyTotal = po.QtyTotal,
                    GrandTotal = po.GrandTotal,

                    UserAccess = po.UserAccess,
                    StatusPO = po.StatusPO,
                    Keterangan = po.Keterangan,
                    CreateDateTime = po.CreateDateTime,

                    Items = po.PurchaseOrderItems
                        .Where(i => i.IsDelete == false || i.IsDelete == null)
                        .Select(i => new PurchaseOrderItemViewModel
                        {
                            ProductName = i.ProductName,
                            Measurement = i.Measurement,
                            Category = i.Category,
                            Qty = i.Qty,
                            Price = i.Price,
                            Discount = i.Discount,
                            SubTotal = i.SubTotal,
                            Keterangan = i.Keterangan
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(new
                {
                    status = "failed",
                    message = "Purchase Order tidak ditemukan"
                });
            }

            return Ok(new
            {
                status = "success",
                message = "Data retrieved successfully",
                data
            });
        }

        [HttpGet("paged")]
        public async Task<IActionResult> Paged(
        int page = 1,
        int perPage = 10,
        string? PRNumber = null,
        string? PONumber = null,
        Guid? supplierId = null,

        [FromQuery(Name = "PurchaseOrderId")]
        Guid? purchaseOrderId = null,

        string? orderBy = "CreateDateTime",
        string? sortDirection = "desc",

        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
        DateTime? startDate = null,

        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
        DateTime? endDate = null,

        [FromQuery, JsonConverter(typeof(StringEnumConverter))]
        PeriodeFilter? periode = null)
        {
            page = page < 1 ? 1 : page;
            perPage = perPage < 1 ? 10 : perPage;
            perPage = perPage > 200 ? 200 : perPage;

            var query = _context.PurchaseOrders
                .AsNoTracking()
                .Where(po => po.IsDelete == false || po.IsDelete == null);

            // ======================================================
            // Filter PurchaseOrderId
            // ======================================================
            if (purchaseOrderId.HasValue)
            {
                query = query.Where(po =>
                    po.PurchaseOrderId == purchaseOrderId.Value);
            }

            // ======================================================
            // Search PR Number
            // ======================================================
            if (!string.IsNullOrWhiteSpace(PRNumber))
            {
                var pattern = $"%{PRNumber.Trim()}%";

                query = query.Where(po =>
                    po.PurchaseRequestNumber != null &&
                    EF.Functions.ILike(po.PurchaseRequestNumber, pattern)
                );
            }

            // ======================================================
            // Search PO Number
            // ======================================================
            if (!string.IsNullOrWhiteSpace(PONumber))
            {
                var pattern = $"%{PONumber.Trim()}%";

                query = query.Where(po =>
                    po.PurchaseOrderNumber != null &&
                    EF.Functions.ILike(po.PurchaseOrderNumber, pattern)
                );
            }

            // ======================================================
            // Filter Supplier
            // ======================================================
            if (supplierId.HasValue)
            {
                query = query.Where(po =>
                    po.SupplierId == supplierId.Value);
            }

            // =====================================================
            // Filter tanggal
            // ======================================================
            if (startDate.HasValue && endDate.HasValue)
            {
                var start = startDate.Value.Date;
                var endExclusive = endDate.Value.Date.AddDays(1);

                query = query.Where(po =>
                    po.CreateDateTime >= start &&
                    po.CreateDateTime < endExclusive);
            }

            // ======================================================
            // Filter periode
            // ======================================================
            if (periode.HasValue)
            {
                DateTime today = DateTime.UtcNow.Date;

                switch (periode.Value)
                {
                    case PeriodeFilter.Today:
                        query = query.Where(po =>
                            po.CreateDateTime >= today &&
                            po.CreateDateTime < today.AddDays(1));
                        break;

                    case PeriodeFilter.ThisWeek:
                        {
                            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
                            var endOfWeek = today.AddDays(1);

                            query = query.Where(po =>
                                po.CreateDateTime >= startOfWeek &&
                                po.CreateDateTime < endOfWeek);
                            break;
                        }

                    case PeriodeFilter.LastWeek:
                        {
                            var startOfThisWeek = today.AddDays(-(int)today.DayOfWeek);
                            var startOfLastWeek = startOfThisWeek.AddDays(-7);

                            query = query.Where(po =>
                                po.CreateDateTime >= startOfLastWeek &&
                                po.CreateDateTime < startOfThisWeek);
                            break;
                        }

                    case PeriodeFilter.ThisMonth:
                        {
                            var startOfMonth = new DateTime(today.Year, today.Month, 1);
                            var startOfNextMonth = startOfMonth.AddMonths(1);

                            query = query.Where(po =>
                                po.CreateDateTime >= startOfMonth &&
                                po.CreateDateTime < startOfNextMonth);
                            break;
                        }

                    case PeriodeFilter.LastMonth:
                        {
                            var startOfThisMonth = new DateTime(today.Year, today.Month, 1);
                            var startOfLastMonth = startOfThisMonth.AddMonths(-1);

                            query = query.Where(po =>
                                po.CreateDateTime >= startOfLastMonth &&
                                po.CreateDateTime < startOfThisMonth);
                            break;
                        }

                    case PeriodeFilter.ThisYear:
                        {
                            var startOfYear = new DateTime(today.Year, 1, 1);
                            var startOfNextYear = startOfYear.AddYears(1);

                            query = query.Where(po =>
                                po.CreateDateTime >= startOfYear &&
                                po.CreateDateTime < startOfNextYear);
                            break;
                        }

                    case PeriodeFilter.LastYear:
                        {
                            var startOfThisYear = new DateTime(today.Year, 1, 1);
                            var startOfLastYear = startOfThisYear.AddYears(-1);

                            query = query.Where(po =>
                                po.CreateDateTime >= startOfLastYear &&
                                po.CreateDateTime < startOfThisYear);
                            break;
                        }

                    case PeriodeFilter.Last3Months:
                        query = query.Where(po =>
                            po.CreateDateTime >= today.AddMonths(-3));
                        break;

                    case PeriodeFilter.Last6Months:
                        query = query.Where(po =>
                            po.CreateDateTime >= today.AddMonths(-6));
                        break;
                }
            }

            // ======================================================
            // Sorting
            // ======================================================
            var sortColumn = orderBy?.ToLower() ?? "createdatetime";
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            query = sortColumn switch
            {
                "purchaseorderid" =>
                    isDesc
                        ? query.OrderByDescending(po => po.PurchaseOrderId)
                        : query.OrderBy(po => po.PurchaseOrderId),

                "purchaserequestnumber" =>
                    isDesc
                        ? query.OrderByDescending(po => po.PurchaseRequestNumber)
                        : query.OrderBy(po => po.PurchaseRequestNumber),

                "purchaseordernumber" =>
                    isDesc
                        ? query.OrderByDescending(po => po.PurchaseOrderNumber)
                        : query.OrderBy(po => po.PurchaseOrderNumber),

                "suppliername" =>
                    isDesc
                        ? query.OrderByDescending(po => po.SupplierName)
                        : query.OrderBy(po => po.SupplierName),

                "grandtotal" =>
                    isDesc
                        ? query.OrderByDescending(po => po.GrandTotal)
                        : query.OrderBy(po => po.GrandTotal),

                "createdatetime" =>
                    isDesc
                        ? query.OrderByDescending(po => po.CreateDateTime)
                        : query.OrderBy(po => po.CreateDateTime),

                _ =>
                    isDesc
                        ? query.OrderByDescending(po => po.CreateDateTime)
                        : query.OrderBy(po => po.CreateDateTime)
            };

            // ======================================================
            // Pagination
            // ======================================================
            var totalRows = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

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

            var rows = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .Select(po => new
                {
                    PurchaseOrderId = po.PurchaseOrderId,
                    PurchaseRequestNumber = po.PurchaseRequestNumber,
                    PurchaseOrderNumber = po.PurchaseOrderNumber,
                    InvoiceDate = po.InvoiceDate,
                    InvoiceNumber = po.InvoiceNumber,
                    RequestType = po.RequestType,

                    SupplierId = po.SupplierId,
                    SupplierName = po.SupplierName,
                    SupplierCode = po.SupplierCode,

                    TermOfPayment = po.TermOfPayment,
                    ExpiredDate = po.ExpiredDate,

                    RemainingDay = po.RemainingDay,
                    QtyTotal = po.QtyTotal,
                    GrandTotal = po.GrandTotal,

                    UserAccess = po.UserAccess,
                    StatusPO = po.StatusPO,
                    Keterangan = po.Keterangan,
                    CreateDateTime = po.CreateDateTime,

                    Items = po.PurchaseOrderItems
                        .Where(i => i.IsDelete == false || i.IsDelete == null)
                        .Select(i => new PurchaseOrderItemViewModel
                        {
                            ProductName = i.ProductName,
                            Measurement = i.Measurement,
                            Category = i.Category,
                            Qty = i.Qty,
                            Price = i.Price,
                            Discount = i.Discount,
                            SubTotal = i.SubTotal,
                            Keterangan = i.Keterangan
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(new
            {
                status = "success",
                message = "Data retrieved successfully",
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
    }
}
