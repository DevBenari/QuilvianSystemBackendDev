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

        [HttpGet("paged")]
        public async Task<IActionResult> Paged(
            int page = 1,
            int perPage = 10,
            string? PRNumber = null,
            string? PONumber = null,
            Guid? supplierId = null,
            Guid? poId = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
            DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
            DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {
            page = page < 1 ? 1 : page;
            perPage = perPage < 1 ? 10 : perPage;
            perPage = perPage > 200 ? 200 : perPage;

            var query = _context.PurchaseOrders
                .AsNoTracking()
                .Where(po => po.IsDelete == false || po.IsDelete == null);

            // ======================================================
            // Search
            // ======================================================
            if (!string.IsNullOrWhiteSpace(PRNumber))
            {
                var pattern = $"%{PRNumber.Trim()}%";

                query = query.Where(po =>
                    (po.PurchaseRequestNumber != null && EF.Functions.ILike(po.PurchaseRequestNumber, pattern))
                );
            }
            if (!string.IsNullOrWhiteSpace(PONumber))
            {
                var pattern = $"%{PONumber.Trim()}%";

                query = query.Where(po =>
                    (po.PurchaseOrderNumber != null && EF.Functions.ILike(po.PurchaseOrderNumber, pattern)) 
                );
            }

            if (supplierId.HasValue)
            {
                query = query.Where(u => u.SupplierId == supplierId.Value);
            }

            if (poId.HasValue)
            {
                query = query.Where(u => u.PurchaseOrderId == poId.Value);
            }

            // =====================================================
            // Filter tanggal
            // Pakai CreateDateTime. Kalau mau berdasarkan InvoiceDate,
            // ganti po.CreateDateTime menjadi po.InvoiceDate.
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
                            po.CreateDateTime.Date == today);
                        break;

                    case PeriodeFilter.ThisWeek:
                        query = query.Where(po =>
                            po.CreateDateTime.Date >= today.AddDays(-(int)today.DayOfWeek) &&
                            po.CreateDateTime.Date <= today);
                        break;

                    case PeriodeFilter.LastWeek:
                        query = query.Where(po =>
                            po.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                            po.CreateDateTime.Date < today.AddDays(-(int)today.DayOfWeek));
                        break;

                    case PeriodeFilter.ThisMonth:
                        query = query.Where(po =>
                            po.CreateDateTime.Month == today.Month &&
                            po.CreateDateTime.Year == today.Year);
                        break;

                    case PeriodeFilter.LastMonth:
                        {
                            var lastMonth = today.AddMonths(-1);

                            query = query.Where(po =>
                                po.CreateDateTime.Month == lastMonth.Month &&
                                po.CreateDateTime.Year == lastMonth.Year);
                            break;
                        }

                    case PeriodeFilter.ThisYear:
                        query = query.Where(po =>
                            po.CreateDateTime.Year == today.Year);
                        break;

                    case PeriodeFilter.LastYear:
                        query = query.Where(po =>
                            po.CreateDateTime.Year == today.Year - 1);
                        break;

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
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            query = isDesc
                ? orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(po => po.CreateDateTime),
                    _ => query.OrderByDescending(po => po.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderBy(po => po.CreateDateTime),
                    _ => query.OrderBy(po => po.CreateDateTime)
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
                return NotFound(new { message = "Page not found." });
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
