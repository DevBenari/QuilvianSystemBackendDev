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
    public class PurchasingInvoiceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PurchasingInvoiceController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PurchasingInvoiceViewModel dto)
        {
            if (dto == null)
                return BadRequest("Data tidak valid");

            if (dto.Items == null || !dto.Items.Any())
                return BadRequest("Detail item tidak boleh kosong");

            var purchasingInvoiceId = Guid.NewGuid();

            var invoice = new Models.PurchasingInvoice
            {
                PurchasingInvoiceId = purchasingInvoiceId,
                POId = dto.POId,
                NoPO = dto.NoPO,
                TglPO = dto.TglPO,
                POAmount = dto.POAmount,
                SupplierId = dto.SupplierId,
                NamaSupplier = dto.NamaSupplier,
                DiskonSupplier = dto.DiskonSupplier,
                SupplierTermPayment = dto.SupplierTermPayment,
                TglPembuatanInvoice = dto.TglPembuatanInvoice,
                TglJatuhTempo = dto.TglJatuhTempo,
                TipePembayaran = dto.TipePembayaran,
                ReceiveOrderId = dto.ReceiveOrderId,
                ReceiveOrderNumber = dto.ReceiveOrderNumber,
                NoInvoice = dto.NoInvoice,
                DownPayment = dto.DownPayment,
                DiskonPersen = dto.DiskonPersen,
                DiskonNominal = dto.DiskonNominal,
                PPNPersen = dto.PPNPersen,
                PPNNominal = dto.PPNNominal,
                OngkosKirim = dto.OngkosKirim,
                Materai = dto.Materai,
                Pembulatan = dto.Pembulatan,
                Potongan = dto.Potongan,
                Retur = dto.Retur,
                OutstandingDP = dto.OutstandingDP,
                COAId = dto.COAId,
                NoFakturPajak = dto.NoFakturPajak,
                TglFaktur = dto.TglFaktur,
                MataUangId = dto.MataUangId,
                NamaMataUang = dto.NamaMataUang,
                RateToIdr = dto.RateToIdr,
                HasilKonversi = dto.HasilKonversi,
                Keterangan = dto.Keterangan,
                CreateDateTime = DateTime.UtcNow,
                IsDelete = false,

                Items = dto.Items.Select(i =>
                {
                    var hargaAkhir = i.HargaAkhir ?? 0;
                    var qty = i.QtyProduk ?? 0;

                    return new ItemPurchasingInvoice
                    {
                        ItemPurchasingInvoiceId = Guid.NewGuid(),
                        PurchasingInvoiceId = purchasingInvoiceId,
                        POId = i.POId ?? dto.POId,
                        ItemPOId = i.ItemPOId,
                        KodeProduk = i.KodeProduk,
                        NamaProduk = i.NamaProduk,
                        QtyProduk = i.QtyProduk,
                        SatuanProduk = i.SatuanProduk,
                        HargaNormal = i.HargaNormal,
                        TipeTax = i.TipeTax,
                        PajakPersen = i.PajakPersen,
                        PajakNominal = i.PajakNominal,
                        HargaAkhir = i.HargaAkhir,
                        HargaTotal = i.HargaTotal ?? hargaAkhir * qty,
                        Keterangan = i.Keterangan,
                        CreateDateTime = DateTime.UtcNow,
                        IsDelete = false
                    };
                }).ToList()
            };

            _context.PurchasingInvoices.Add(invoice);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Purchasing Invoice berhasil disimpan",
                purchasingInvoiceId = invoice.PurchasingInvoiceId
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.PurchasingInvoices
                .AsNoTracking()
                .Include(x => x.Items)
                .Where(x => x.IsDelete == false || x.IsDelete == null)
                .OrderByDescending(x => x.CreateDateTime)
                .Select(x => new PurchasingInvoiceViewModel
                {
                    PurchasingInvoiceId = x.PurchasingInvoiceId,
                    POId = x.POId,
                    NoPO = x.NoPO,
                    TglPO = x.TglPO,
                    POAmount = x.POAmount,
                    SupplierId = x.SupplierId,
                    NamaSupplier = x.NamaSupplier,
                    DiskonSupplier = x.DiskonSupplier,
                    SupplierTermPayment = x.SupplierTermPayment,
                    TglPembuatanInvoice = x.TglPembuatanInvoice,
                    TglJatuhTempo = x.TglJatuhTempo,
                    TipePembayaran = x.TipePembayaran,
                    ReceiveOrderId = x.ReceiveOrderId,
                    ReceiveOrderNumber = x.ReceiveOrderNumber,
                    NoInvoice = x.NoInvoice,
                    DownPayment = x.DownPayment,
                    DiskonPersen = x.DiskonPersen,
                    DiskonNominal = x.DiskonNominal,
                    PPNPersen = x.PPNPersen,
                    PPNNominal = x.PPNNominal,
                    OngkosKirim = x.OngkosKirim,
                    Materai = x.Materai,
                    Pembulatan = x.Pembulatan,
                    Potongan = x.Potongan,
                    Retur = x.Retur,
                    OutstandingDP = x.OutstandingDP,
                    COAId = x.COAId,
                    NoFakturPajak = x.NoFakturPajak,
                    TglFaktur = x.TglFaktur,
                    MataUangId = x.MataUangId,
                    NamaMataUang = x.NamaMataUang,
                    RateToIdr = x.RateToIdr,
                    HasilKonversi = x.HasilKonversi,
                    Keterangan = x.Keterangan,

                    Items = x.Items
                        .Where(i => i.IsDelete == false || i.IsDelete == null)
                        .Select(i => new ItemPurchasingInvoiceViewModel
                        {
                            ItemPurchasingInvoiceId = i.ItemPurchasingInvoiceId,
                            PurchasingInvoiceId = i.PurchasingInvoiceId,
                            POId = i.POId,
                            ItemPOId = i.ItemPOId,
                            KodeProduk = i.KodeProduk,
                            NamaProduk = i.NamaProduk,
                            QtyProduk = i.QtyProduk,
                            SatuanProduk = i.SatuanProduk,
                            HargaNormal = i.HargaNormal,
                            TipeTax = i.TipeTax,
                            PajakPersen = i.PajakPersen,
                            PajakNominal = i.PajakNominal,
                            HargaAkhir = i.HargaAkhir,
                            HargaTotal = i.HargaTotal,
                            Keterangan = i.Keterangan
                        }).ToList()
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _context.PurchasingInvoices
                .AsNoTracking()
                .Include(x => x.Items)
                .Where(x =>
                    x.PurchasingInvoiceId == id &&
                    (x.IsDelete == false || x.IsDelete == null))
                .Select(x => new PurchasingInvoiceViewModel
                {
                    PurchasingInvoiceId = x.PurchasingInvoiceId,
                    POId = x.POId,
                    NoPO = x.NoPO,
                    TglPO = x.TglPO,
                    POAmount = x.POAmount,
                    SupplierId = x.SupplierId,
                    NamaSupplier = x.NamaSupplier,
                    DiskonSupplier = x.DiskonSupplier,
                    SupplierTermPayment = x.SupplierTermPayment,
                    TglPembuatanInvoice = x.TglPembuatanInvoice,
                    TglJatuhTempo = x.TglJatuhTempo,
                    TipePembayaran = x.TipePembayaran,
                    ReceiveOrderId = x.ReceiveOrderId,
                    ReceiveOrderNumber = x.ReceiveOrderNumber,
                    NoInvoice = x.NoInvoice,
                    DownPayment = x.DownPayment,
                    DiskonPersen = x.DiskonPersen,
                    DiskonNominal = x.DiskonNominal,
                    PPNPersen = x.PPNPersen,
                    PPNNominal = x.PPNNominal,
                    OngkosKirim = x.OngkosKirim,
                    Materai = x.Materai,
                    Pembulatan = x.Pembulatan,
                    Potongan = x.Potongan,
                    Retur = x.Retur,
                    OutstandingDP = x.OutstandingDP,
                    COAId = x.COAId,
                    NoFakturPajak = x.NoFakturPajak,
                    TglFaktur = x.TglFaktur,
                    MataUangId = x.MataUangId,
                    NamaMataUang = x.NamaMataUang,
                    RateToIdr = x.RateToIdr,
                    HasilKonversi = x.HasilKonversi,
                    Keterangan = x.Keterangan,

                    Items = x.Items
                        .Where(i => i.IsDelete == false || i.IsDelete == null)
                        .Select(i => new ItemPurchasingInvoiceViewModel
                        {
                            ItemPurchasingInvoiceId = i.ItemPurchasingInvoiceId,
                            PurchasingInvoiceId = i.PurchasingInvoiceId,
                            POId = i.POId,
                            ItemPOId = i.ItemPOId,
                            KodeProduk = i.KodeProduk,
                            NamaProduk = i.NamaProduk,
                            QtyProduk = i.QtyProduk,
                            SatuanProduk = i.SatuanProduk,
                            HargaNormal = i.HargaNormal,
                            TipeTax = i.TipeTax,
                            PajakPersen = i.PajakPersen,
                            PajakNominal = i.PajakNominal,
                            HargaAkhir = i.HargaAkhir,
                            HargaTotal = i.HargaTotal,
                            Keterangan = i.Keterangan
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            if (data == null)
                return NotFound("Data Purchasing Invoice tidak ditemukan");

            return Ok(data);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PurchasingInvoiceViewModel dto)
        {
            var invoice = await _context.PurchasingInvoices
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x =>
                    x.PurchasingInvoiceId == id &&
                    (x.IsDelete == false || x.IsDelete == null));

            if (invoice == null)
                return NotFound("Data Purchasing Invoice tidak ditemukan");

            invoice.POId = dto.POId;
            invoice.NoPO = dto.NoPO;
            invoice.TglPO = dto.TglPO;
            invoice.POAmount = dto.POAmount;
            invoice.SupplierId = dto.SupplierId;
            invoice.NamaSupplier = dto.NamaSupplier;
            invoice.DiskonSupplier = dto.DiskonSupplier;
            invoice.SupplierTermPayment = dto.SupplierTermPayment;
            invoice.TglPembuatanInvoice = dto.TglPembuatanInvoice;
            invoice.TglJatuhTempo = dto.TglJatuhTempo;
            invoice.TipePembayaran = dto.TipePembayaran;
            invoice.ReceiveOrderId = dto.ReceiveOrderId;
            invoice.ReceiveOrderNumber = dto.ReceiveOrderNumber;
            invoice.NoInvoice = dto.NoInvoice;
            invoice.DownPayment = dto.DownPayment;
            invoice.DiskonPersen = dto.DiskonPersen;
            invoice.DiskonNominal = dto.DiskonNominal;
            invoice.PPNPersen = dto.PPNPersen;
            invoice.PPNNominal = dto.PPNNominal;
            invoice.OngkosKirim = dto.OngkosKirim;
            invoice.Materai = dto.Materai;
            invoice.Pembulatan = dto.Pembulatan;
            invoice.Potongan = dto.Potongan;
            invoice.Retur = dto.Retur;
            invoice.OutstandingDP = dto.OutstandingDP;
            invoice.COAId = dto.COAId;
            invoice.NoFakturPajak = dto.NoFakturPajak;
            invoice.TglFaktur = dto.TglFaktur;
            invoice.MataUangId = dto.MataUangId;
            invoice.NamaMataUang = dto.NamaMataUang;
            invoice.RateToIdr = dto.RateToIdr;
            invoice.HasilKonversi = dto.HasilKonversi;
            invoice.Keterangan = dto.Keterangan;
            invoice.UpdateDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Purchasing Invoice berhasil diupdate",
                purchasingInvoiceId = invoice.PurchasingInvoiceId
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var invoice = await _context.PurchasingInvoices
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x =>
                    x.PurchasingInvoiceId == id &&
                    (x.IsDelete == false || x.IsDelete == null));

            if (invoice == null)
                return NotFound("Data Purchasing Invoice tidak ditemukan");

            invoice.IsDelete = true;
            invoice.UpdateDateTime = DateTime.UtcNow;

            foreach (var item in invoice.Items)
            {
                item.IsDelete = true;
                item.UpdateDateTime = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Purchasing Invoice berhasil dihapus"
            });
        }
    }

}
