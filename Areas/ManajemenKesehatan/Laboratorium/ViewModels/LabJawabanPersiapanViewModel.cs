namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels
{
    public class LabJawabanPersiapanViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public Guid? PemeriksaanLabId { get; set; }
        public Guid? LabPersiapanPemeriksaanId { get; set; }
        public bool? IsJawabanPersiapan { get; set; }
        public string? Keterangan { get; set; }
    }
}
