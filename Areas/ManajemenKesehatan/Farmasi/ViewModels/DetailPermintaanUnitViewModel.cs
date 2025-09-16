namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels
{
    public class DetailPermintaanUnitViewModel
    {
        public Guid? PermintaanUnitId { get; set; }
        public Guid? ObatId { get; set; }
        public decimal? QtyPermintaan { get; set; }
        public string? SatuanItem { get; set; }
        public string? KategoriItem { get; set; }
        public string? Keterangan { get; set; }
    }
}
