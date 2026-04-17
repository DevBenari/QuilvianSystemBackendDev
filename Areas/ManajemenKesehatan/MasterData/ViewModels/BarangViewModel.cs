namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class BarangViewModel
    {       

        public Guid? ItemId { get; set; }
        public string? NamaBarang { get; set; }
        public Guid? KategoriBarangId { get; set; }
        public Guid? BrandId { get; set; }
        public Guid? KelasResikoId { get; set; }
        public string? Spesifikasi { get; set; }
        public bool? IsPerluResep { get; set; }
        public decimal? StokMinimum { get; set; }
        public decimal? StokMaximum { get; set; }
        public string? Keterangan { get; set; }
    }
}
