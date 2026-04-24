namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels
{
    public class DetailPenerimaanUnitViewModel
    {
        public Guid? PenerimaanUnitId { get; set; }
        public Guid? ObatId { get; set; }
        public decimal? QtyPermintaan { get; set; }
        public string? SatuanItem { get; set; }
        public string? KategoriItem { get; set; }
        public string? Keterangan { get; set; }
    }
}
