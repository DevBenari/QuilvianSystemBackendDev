namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels
{
    public class MainKasirDetailViewModel
    {
        public Guid? MainKasirId { get; set; }
        public Guid? MetodePembayaranId { get; set; }
        public Guid? ReferenceId { get; set; }
        public string? NamaMetode { get; set; }
        public decimal? NominalPembayaran { get; set; }
        public string? Keterangan { get; set; }
    }
}
