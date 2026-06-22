namespace QuilvianSystemBackendDev.Areas.Finance.AP.ViewModels
{
    public class PembayaranAPViewModel
    {
        public Guid? PembayaranAPId { get; set; }

        public string? KodePembayaranAP { get; set; }

        public string? NoReferensi { get; set; }

        public decimal? TotalTagihan { get; set; }

        public Guid? SupplierId { get; set; }

        public string? NamaSupplier { get; set; }

        public string? NoTukarFaktur { get; set; }

        public DateTime? TglPembayaranAP { get; set; }

        public Guid? BankId { get; set; }

        public string? NamaBank { get; set; }

        public DateTime? TglJatuhTempo { get; set; }

        public string? TipePembayaran { get; set; }

        public string? StatusPembayaran { get; set; }

        public decimal? Potongan { get; set; }

        public string? Keterangan { get; set; }

        public int? NoRekening { get; set; }

        public List<DetailPembayaranAPViewModel> Details { get; set; } = new();
    }
}