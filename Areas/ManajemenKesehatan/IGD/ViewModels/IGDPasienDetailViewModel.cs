namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.ViewModels
{
    public class IGDPasienDetailViewModel
    {
        public Guid? KunjunganId { get; set; }
        public string? JenisKasus { get; set; }
        public string? JenisEmergency { get; set; }
        public string? KategoriPenyakit { get; set; }
        public string? AlasanKeluar { get; set; }
        public string? LokasiTrauma { get; set; }
        public DateTime? TanggalTrauma { get; set; }
        public string? Keterangan { get; set; }
    }
}
