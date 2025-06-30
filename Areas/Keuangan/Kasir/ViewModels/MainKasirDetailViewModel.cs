namespace QuilvianSystemBackendDev.Areas.Keuangan.Kasir.ViewModels
{
    public class MainKasirDetailViewModel
    {
        public Guid? MainKasirId { get; set; }
        public Guid? MetodePembayaranId { get; set; }
        public Guid? ReferenceId { get; set; }
        public string? NamaMetode { get; set; }
        public decimal? NominalPembayaran { get; set; }
        public string? Keterangan { get; set; }
        public bool? StatusPembayaran { get; set; }
    }
}
