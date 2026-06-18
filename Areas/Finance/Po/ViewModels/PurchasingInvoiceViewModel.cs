namespace QuilvianSystemBackendDev.Areas.Finance.Po.ViewModels
{
    public class PurchasingInvoiceViewModel
    {
        public Guid? PurchasingInvoiceId { get; set; }

        public Guid? POId { get; set; }

        public string? NoPO { get; set; }

        public DateTime? TglPO { get; set; }

        public decimal? POAmount { get; set; }

        public Guid? SupplierId { get; set; }

        public string? NamaSupplier { get; set; }

        public decimal? DiskonSupplier { get; set; }

        public int? SupplierTermPayment { get; set; }

        public DateTime? TglPembuatanInvoice { get; set; }

        public DateTime? TglJatuhTempo { get; set; }

        public string? TipePembayaran { get; set; }

        public Guid? ReceiveOrderId { get; set; }

        public string? ReceiveOrderNumber { get; set; }

        public string? NoInvoice { get; set; }

        public decimal? DownPayment { get; set; }

        public decimal? DiskonPersen { get; set; }

        public decimal? DiskonNominal { get; set; }

        public decimal? PPNPersen { get; set; }

        public decimal? PPNNominal { get; set; }

        public decimal? OngkosKirim { get; set; }

        public decimal? Materai { get; set; }

        public decimal? Pembulatan { get; set; }

        public decimal? Potongan { get; set; }

        public decimal? Retur { get; set; }

        public decimal? OutstandingDP { get; set; }

        public Guid? COAId { get; set; }

        public string? NoFakturPajak { get; set; }

        public DateTime? TglFaktur { get; set; }

        public Guid? MataUangId { get; set; }

        public string? NamaMataUang { get; set; }

        public decimal? RateToIdr { get; set; }

        public decimal? HasilKonversi { get; set; }

        public string? Keterangan { get; set; }
        public string? KodePurchasingInvoice { get; set; }

        public Guid? CreateBy { get; set; }

        public string? Status { get; set; }

        public List<ItemPurchasingInvoiceViewModel> Items { get; set; } = new();
    }

}
