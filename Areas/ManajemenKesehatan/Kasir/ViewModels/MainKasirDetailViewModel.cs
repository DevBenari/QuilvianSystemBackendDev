namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels
{
    public class MainKasirDetailViewModel
    {
        public Guid? MainKasirId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public decimal? TotalPembayaran { get; set; }
        public decimal? SisaPembayaran { get; set; }
        public string? InvoiceBilling { get; set; }
        public decimal? AngsuranKe { get; set; }
        public Guid? MetodePembayaranId { get; set; }
        public Guid? ReferenceId { get; set; }
        public string? NamaMetode { get; set; }
        public decimal? NominalPembayaran { get; set; }
        public string? Keterangan { get; set; }
    }
}
