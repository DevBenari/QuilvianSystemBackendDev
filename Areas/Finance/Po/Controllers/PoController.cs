using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.Finance.Po.Models;
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

        // =======================
        // POST Purchase Order SAJA
        // POST: api/Finance/Po
        // =======================
        [HttpPost]
        public async Task<IActionResult> CreatePo([FromBody] PurchaseOrder po)
        {
            po.PurchaseOrderId = Guid.NewGuid();

            _context.PurchaseOrders.Add(po);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Purchase Order berhasil dibuat",
                po.PurchaseOrderId
            });
        }

        // =======================
        // POST Purchase Order Item
        // POST: api/Finance/Po/item
        // =======================
        [HttpPost("item")]
        public async Task<IActionResult> CreatePoItem([FromBody] PurchaseOrderItem item)
        {
            item.PurchaseOrderItemId = Guid.NewGuid();

            _context.PurchaseOrderItems.Add(item);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Item berhasil dibuat",
                item.PurchaseOrderItemId
            });
        }

        // =======================
        // GET PO + Items
        // =======================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.PurchaseOrders
                .Select(x => new
                {
                    x.PurchaseOrderId,
                    x.PurchaseOrderNumber,
                    x.PurchaseRequestId,
                    x.PurchaseRequestNumber,
                    x.RequestType,
                    x.SupplierId,
                    x.TermOfPaymentId,
                    x.ExpiredDate,
                    x.RemainingDay,
                    x.QtyTotal,
                    x.GrandTotal,
                    x.Keterangan
                })
                .ToListAsync();

            return Ok(data);
        }


        // =======================
        // GET Items by PO Id
        // GET: api/Finance/Po/{poId}/items
        // =======================
        [HttpGet("item")]
        public async Task<IActionResult> GetAllItems()
        {
            var data = await _context.PurchaseOrderItems
                .Select(x => new
                {
                    x.PurchaseOrderItemId,
                    x.PurchaseOrderId,
                    x.ListPurchaseRequestId,
                    x.ProductId,
                    x.ProductName,
                    x.Measurement,
                    x.Category,
                    x.Layanan,
                    x.JenisPermintaan,
                    x.Qty,
                    x.Price,
                    x.Discount,
                    x.SubTotal,
                    x.Keterangan
                })
                .ToListAsync();

            return Ok(data);
        }

    }
}
