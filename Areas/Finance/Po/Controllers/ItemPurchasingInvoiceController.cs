using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.Finance.Po.Models;
using QuilvianSystemBackendDev.Areas.Finance.Po.ViewModels;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.Finance.Po.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class ItemPurchasingInvoiceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ItemPurchasingInvoiceController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ItemPurchasingInvoiceViewModel dto)
        {
            if (dto == null)
                return BadRequest("Data tidak valid");

            if (dto.PurchasingInvoiceId == null)
                return BadRequest("PurchasingInvoiceId wajib diisi");

            var purchasingInvoiceExists = await _context.PurchasingInvoices
                .AnyAsync(x =>
                    x.PurchasingInvoiceId == dto.PurchasingInvoiceId &&
                    (x.IsDelete == false || x.IsDelete == null));

            if (!purchasingInvoiceExists)
                return NotFound("Data Purchasing Invoice tidak ditemukan");

            var hargaAkhir = dto.HargaAkhir ?? 0;
            var qty = dto.QtyProduk ?? 0;

            var item = new ItemPurchasingInvoice
            {
                ItemPurchasingInvoiceId = Guid.NewGuid(),
                PurchasingInvoiceId = dto.PurchasingInvoiceId.Value,
                POId = dto.POId,
                ItemPOId = dto.ItemPOId,
                KodeProduk = dto.KodeProduk,
                NamaProduk = dto.NamaProduk,
                QtyProduk = dto.QtyProduk,
                SatuanProduk = dto.SatuanProduk,
                HargaNormal = dto.HargaNormal,
                TipeTax = dto.TipeTax,
                PajakPersen = dto.PajakPersen,
                PajakNominal = dto.PajakNominal,
                HargaAkhir = dto.HargaAkhir,
                HargaTotal = dto.HargaTotal ?? hargaAkhir * qty,
                Keterangan = dto.Keterangan,
                CreateDateTime = DateTime.UtcNow,
                IsDelete = false
            };

            _context.ItemPurchasingInvoices.Add(item);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Item Purchasing Invoice berhasil disimpan",
                itemPurchasingInvoiceId = item.ItemPurchasingInvoiceId
            });
        }

        [HttpGet("by-invoice/{purchasingInvoiceId}")]
        public async Task<IActionResult> GetByPurchasingInvoiceId(Guid purchasingInvoiceId)
        {
            var data = await _context.ItemPurchasingInvoices
                .AsNoTracking()
                .Where(x =>
                    x.PurchasingInvoiceId == purchasingInvoiceId &&
                    (x.IsDelete == false || x.IsDelete == null))
                .Select(x => new ItemPurchasingInvoiceViewModel
                {
                    ItemPurchasingInvoiceId = x.ItemPurchasingInvoiceId,
                    PurchasingInvoiceId = x.PurchasingInvoiceId,
                    POId = x.POId,
                    ItemPOId = x.ItemPOId,
                    KodeProduk = x.KodeProduk,
                    NamaProduk = x.NamaProduk,
                    QtyProduk = x.QtyProduk,
                    SatuanProduk = x.SatuanProduk,
                    HargaNormal = x.HargaNormal,
                    TipeTax = x.TipeTax,
                    PajakPersen = x.PajakPersen,
                    PajakNominal = x.PajakNominal,
                    HargaAkhir = x.HargaAkhir,
                    HargaTotal = x.HargaTotal,
                    Keterangan = x.Keterangan
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _context.ItemPurchasingInvoices
                .AsNoTracking()
                .Where(x =>
                    x.ItemPurchasingInvoiceId == id &&
                    (x.IsDelete == false || x.IsDelete == null))
                .Select(x => new ItemPurchasingInvoiceViewModel
                {
                    ItemPurchasingInvoiceId = x.ItemPurchasingInvoiceId,
                    PurchasingInvoiceId = x.PurchasingInvoiceId,
                    POId = x.POId,
                    ItemPOId = x.ItemPOId,
                    KodeProduk = x.KodeProduk,
                    NamaProduk = x.NamaProduk,
                    QtyProduk = x.QtyProduk,
                    SatuanProduk = x.SatuanProduk,
                    HargaNormal = x.HargaNormal,
                    TipeTax = x.TipeTax,
                    PajakPersen = x.PajakPersen,
                    PajakNominal = x.PajakNominal,
                    HargaAkhir = x.HargaAkhir,
                    HargaTotal = x.HargaTotal,
                    Keterangan = x.Keterangan
                })
                .FirstOrDefaultAsync();

            if (data == null)
                return NotFound("Item Purchasing Invoice tidak ditemukan");

            return Ok(data);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ItemPurchasingInvoiceViewModel dto)
        {
            var item = await _context.ItemPurchasingInvoices
                .FirstOrDefaultAsync(x =>
                    x.ItemPurchasingInvoiceId == id &&
                    (x.IsDelete == false || x.IsDelete == null));

            if (item == null)
                return NotFound("Item Purchasing Invoice tidak ditemukan");

            var hargaAkhir = dto.HargaAkhir ?? 0;
            var qty = dto.QtyProduk ?? 0;

            item.POId = dto.POId;
            item.ItemPOId = dto.ItemPOId;
            item.KodeProduk = dto.KodeProduk;
            item.NamaProduk = dto.NamaProduk;
            item.QtyProduk = dto.QtyProduk;
            item.SatuanProduk = dto.SatuanProduk;
            item.HargaNormal = dto.HargaNormal;
            item.TipeTax = dto.TipeTax;
            item.PajakPersen = dto.PajakPersen;
            item.PajakNominal = dto.PajakNominal;
            item.HargaAkhir = dto.HargaAkhir;
            item.HargaTotal = dto.HargaTotal ?? hargaAkhir * qty;
            item.Keterangan = dto.Keterangan;
            item.UpdateDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Item Purchasing Invoice berhasil diupdate",
                itemPurchasingInvoiceId = item.ItemPurchasingInvoiceId
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var item = await _context.ItemPurchasingInvoices
                .FirstOrDefaultAsync(x =>
                    x.ItemPurchasingInvoiceId == id &&
                    (x.IsDelete == false || x.IsDelete == null));

            if (item == null)
                return NotFound("Item Purchasing Invoice tidak ditemukan");

            item.IsDelete = true;
            item.UpdateDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Item Purchasing Invoice berhasil dihapus"
            });
        }
    }

}
