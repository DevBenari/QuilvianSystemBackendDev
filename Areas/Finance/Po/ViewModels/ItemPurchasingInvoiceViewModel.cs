namespace QuilvianSystemBackendDev.Areas.Finance.Po.ViewModels
{
    public class ItemPurchasingInvoiceViewModel
    {
        public Guid? ItemPurchasingInvoiceId { get; set; }

        public Guid? PurchasingInvoiceId { get; set; }

        public Guid? POId { get; set; }

        public Guid? ItemPOId { get; set; }

        public string? KodeProduk { get; set; }

        public string? NamaProduk { get; set; }

        public decimal? QtyProduk { get; set; }

        public string? SatuanProduk { get; set; }

        public decimal? HargaNormal { get; set; }

        public string? TipeTax { get; set; }

        public decimal? PajakPersen { get; set; }

        public decimal? PajakNominal { get; set; }

        public decimal? HargaAkhir { get; set; }

        public decimal? HargaTotal { get; set; }

        public string? Keterangan { get; set; }
    }
}
