namespace QuilvianSystemBackendDev.Areas.Finance.AP.ViewModels
{
    public class DetailPembayaranAPViewModel
    {
        public Guid? DetailPembayaranAPId { get; set; }

        public Guid? PembayaranAPId { get; set; }

        public Guid? PurchasingInvoiceId { get; set; }

        public string? KodePurchasingInvoice { get; set; }

        public DateTime? TglPembuatanInvoice { get; set; }

        public string? NoInvoice { get; set; }

        public string? NoTukarFaktur { get; set; }

        public decimal? TotalTagihan { get; set; }

        public decimal? SisaTagihan { get; set; }

        public decimal? PembayaranTagihan { get; set; }

        public string? Keterangan { get; set; }
    }
}