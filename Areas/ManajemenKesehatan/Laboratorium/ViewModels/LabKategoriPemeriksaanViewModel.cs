namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels
{
    public class LabKategoriPemeriksaanViewModel
    {
        public string? NamaKategori { get; set; }
        public string? KodeKategori { get; set; }
        public Guid? LabId { get; set; }
        public string? Keterangan { get; set; } // Keterangan tambahan
    }
}
