namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels
{
    public class LabPemeriksaanPersiapanViewModel
    {
        public Guid? LabId { get; set; }
        public Guid? PemeriksaanLabId { get; set; }
        public Guid? LabPersiapanPemeriksaanId { get; set; }
        public string? Keterangan { get; set; }
    }
}
