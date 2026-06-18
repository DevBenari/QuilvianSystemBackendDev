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
                .Where(x => x.IsDelete == false || x.IsDelete == null)
                .OrderByDescending(x => x.CreateDateTime)
                .Select(x => new PurchasingInvoiceViewModel
                {
                    PurchasingInvoiceId = x.PurchasingInvoiceId,

                    // Ambil dari DetailTukarFaktur berdasarkan POId
                    KodePurchasingInvoice = _context.DetailTukarFakturs
                        .Where(d =>
                            d.POId == x.POId &&
                            (d.IsDelete == false || d.IsDelete == null))
                        .Select(d => d.KodePurchasingInvoice)
                        .FirstOrDefault(),

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

                    // CreateBy dari UserActivity
                    CreateBy = x.CreateBy,

                    // Status dari model PurchasingInvoice
                    Status = x.Status,

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
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> Paged(
        int page = 1,
        int perPage = 10,
        string? noPO = null,
        string? noInvoice = null,
        string? kodePurchasingInvoice = null,
        Guid? supplierId = null,
        Guid? poId = null,
        string? namaSupplier = null,
        string? orderBy = "CreateDateTime",
        string? sortDirection = "desc",
        DateTime? startDate = null,
        DateTime? endDate = null)
        {
            page = page < 1 ? 1 : page;
            perPage = perPage < 1 ? 10 : perPage;
            perPage = perPage > 200 ? 200 : perPage;

            var query = _context.PurchasingInvoices
                .AsNoTracking()
                .Include(x => x.Items)
                .Where(x => x.IsDelete == false || x.IsDelete == null);

            if (!string.IsNullOrWhiteSpace(noPO))
            {
                var pattern = $"%{noPO.Trim()}%";

                query = query.Where(x =>
                    x.NoPO != null &&
                    EF.Functions.ILike(x.NoPO, pattern));
            }

            if (!string.IsNullOrWhiteSpace(noInvoice))
            {
                var pattern = $"%{noInvoice.Trim()}%";

                query = query.Where(x =>
                    x.NoInvoice != null &&
                    EF.Functions.ILike(x.NoInvoice, pattern));
            }

            // =========================
            // Filter KodePurchasingInvoice
            // dari DetailTukarFaktur berdasarkan POId
            // =========================
            if (!string.IsNullOrWhiteSpace(kodePurchasingInvoice))
            {
                var pattern = $"%{kodePurchasingInvoice.Trim()}%";

                query = query.Where(x =>
                    _context.DetailTukarFakturs.Any(d =>
                        d.POId == x.POId &&
                        (d.IsDelete == false || d.IsDelete == null) &&
                        d.KodePurchasingInvoice != null &&
                        EF.Functions.ILike(d.KodePurchasingInvoice, pattern)
                    )
                );
            }

            if (!string.IsNullOrWhiteSpace(namaSupplier))
            {
                var pattern = $"%{namaSupplier.Trim()}%";

                query = query.Where(x =>
                    x.NamaSupplier != null &&
                    EF.Functions.ILike(x.NamaSupplier, pattern));
            }

            if (supplierId.HasValue)
            {
                query = query.Where(x => x.SupplierId == supplierId.Value);
            }

            if (poId.HasValue)
            {
                query = query.Where(x => x.POId == poId.Value);
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                var start = startDate.Value.Date;
                var endExclusive = endDate.Value.Date.AddDays(1);

                query = query.Where(x =>
                    x.CreateDateTime >= start &&
                    x.CreateDateTime < endExclusive);
            }

            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            query = isDesc
                ? orderBy switch
                {
                    "NoPO" => query.OrderByDescending(x => x.NoPO),

                    "NoInvoice" => query.OrderByDescending(x => x.NoInvoice),

                    "KodePurchasingInvoice" => query.OrderByDescending(x =>
                        _context.DetailTukarFakturs
                            .Where(d =>
                                d.POId == x.POId &&
                                (d.IsDelete == false || d.IsDelete == null))
                            .Select(d => d.KodePurchasingInvoice)
                            .FirstOrDefault()),

                    "NamaSupplier" => query.OrderByDescending(x => x.NamaSupplier),

                    "TglPO" => query.OrderByDescending(x => x.TglPO),

                    "TglPembuatanInvoice" => query.OrderByDescending(x => x.TglPembuatanInvoice),

                    "Status" => query.OrderByDescending(x => x.Status),

                    _ => query.OrderByDescending(x => x.CreateDateTime)
                }
                : orderBy switch
                {
                    "NoPO" => query.OrderBy(x => x.NoPO),

                    "NoInvoice" => query.OrderBy(x => x.NoInvoice),

                    "KodePurchasingInvoice" => query.OrderBy(x =>
                        _context.DetailTukarFakturs
                            .Where(d =>
                                d.POId == x.POId &&
                                (d.IsDelete == false || d.IsDelete == null))
                            .Select(d => d.KodePurchasingInvoice)
                            .FirstOrDefault()),

                    "NamaSupplier" => query.OrderBy(x => x.NamaSupplier),

                    "TglPO" => query.OrderBy(x => x.TglPO),

                    "TglPembuatanInvoice" => query.OrderBy(x => x.TglPembuatanInvoice),

                    "Status" => query.OrderBy(x => x.Status),

                    _ => query.OrderBy(x => x.CreateDateTime)
                };

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
                .Select(x => new
                {
                    x.PurchasingInvoiceId,

                    KodePurchasingInvoice = _context.DetailTukarFakturs
                        .Where(d =>
                            d.POId == x.POId &&
                            (d.IsDelete == false || d.IsDelete == null))
                        .Select(d => d.KodePurchasingInvoice)
                        .FirstOrDefault(),

                    x.POId,
                    x.NoPO,
                    x.TglPO,
                    x.POAmount,

                    x.SupplierId,
                    x.NamaSupplier,
                    x.DiskonSupplier,
                    x.SupplierTermPayment,

                    x.TglPembuatanInvoice,
                    x.TglJatuhTempo,
                    x.TipePembayaran,

                    x.ReceiveOrderId,
                    x.ReceiveOrderNumber,
                    x.NoInvoice,

                    x.DownPayment,
                    x.DiskonPersen,
                    x.DiskonNominal,
                    x.PPNPersen,
                    x.PPNNominal,
                    x.OngkosKirim,
                    x.Materai,
                    x.Pembulatan,
                    x.Potongan,
                    x.Retur,
                    x.OutstandingDP,

                    x.COAId,
                    x.NoFakturPajak,
                    x.TglFaktur,

                    x.MataUangId,
                    x.NamaMataUang,
                    x.RateToIdr,
                    x.HasilKonversi,

                    x.Keterangan,
                    x.Status,
                    x.CreateBy,
                    x.CreateDateTime,

                    Items = x.Items
                        .Where(i => i.IsDelete == false || i.IsDelete == null)
                        .Select(i => new
                        {
                            i.ItemPurchasingInvoiceId,
                            i.PurchasingInvoiceId,
                            i.POId,
                            i.ItemPOId,
                            i.KodeProduk,
                            i.NamaProduk,
                            i.QtyProduk,
                            i.SatuanProduk,
                            i.HargaNormal,
                            i.TipeTax,
                            i.PajakPersen,
                            i.PajakNominal,
                            i.HargaAkhir,
                            i.HargaTotal,
                            i.Keterangan
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
