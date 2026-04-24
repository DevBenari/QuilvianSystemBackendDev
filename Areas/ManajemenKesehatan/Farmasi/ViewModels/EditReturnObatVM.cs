namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels
{
    public class EditReturnObatVM
    {
        // buat return obat
        public bool? IsReturn { get; set; }
        public decimal? QtyReturn { get; set; }
        public string? AlasanReturn { get; set; }
        public Guid? DikembalikanOleh { get; set; }
    }
}
