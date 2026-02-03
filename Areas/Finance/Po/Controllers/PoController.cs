using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.Finance.Po.Models;
using QuilvianSystemBackendDev.Areas.Finance.Po.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Repositories;
using System;

namespace QuilvianSystemBackendDev.Areas.Finance.Po.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
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
        public async Task<IActionResult> Create(PurchaseOrderViewModel dto)
        {
            // ===============================
            // 1. Cek / Insert Supplier
            // ===============================
            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(x => x.SupplierCode == dto.SupplierCode);

            if (supplier == null)
            {
                supplier = new Supplier
                {
                    SupplierId = Guid.NewGuid(),
                    SupplierCode = dto.SupplierCode!,
                    SupplierName = dto.SupplierName!,
                    IsActive = true
                };

                _context.Suppliers.Add(supplier);
                await _context.SaveChangesAsync(); // simpan supplier dulu
            }

            // ===============================
            // 2. Insert Purchase Order
            // ===============================
            var po = new PurchaseOrder
            {
                PurchaseOrderId = Guid.NewGuid(),

                SupplierCode = supplier.SupplierCode,
                SupplierName = supplier.SupplierName,

                PurchaseRequestNumber = dto.PurchaseRequestNumber,
                PurchaseOrderNumber = dto.PurchaseOrderNumber,
                InvoiceDate = dto.InvoiceDate,
                InvoiceNumber = dto.InvoiceNumber,
                RequestType = dto.RequestType,

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
                message = "Purchase Order & Supplier berhasil disimpan",
                purchaseOrderId = po.PurchaseOrderId,
                supplierId = supplier.SupplierId
            });
        }


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

    }
}
