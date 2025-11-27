namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.ViewModels
{
    public class InfeksiLOViewModel
    {
        public Guid? KunjunganId { get; set; }  // Relasi ke tabel Kunjungan
        public Guid? PasienId { get; set; }     // Relasi ke tabel Pendaftaran Pasien Baru
        public bool? IsDarurat { get; set; }  // True jika operasi darurat
        public bool? IsAnastesiUmum { get; set; }  // True jika tipe anastesi = Umum
        public string? RondeKe { get; set; }  // Penomoran ronde (otomatis naik)
        public bool? IsTrauma { get; set; }  // True jika kasus trauma
        public bool? IsProsedurMultiple { get; set; }  // True jika operasi ganda
        public decimal? ASAScore { get; set; }  // Nilai ASA Score (numeric)
        public bool? IsHbsag { get; set; }  // Hasil lab HBsAg
        public bool? IsAntiHCV { get; set; }  // Hasil lab Anti-HCV
        public string? HasilLabLeukosit { get; set; }  // Nilai lab leukosit
        public string? HasilLabHB { get; set; }  // Nilai lab Hb
        public DateTime? TglPencatatan { get; set; }  // Waktu pencatatan form
        public string? Keterangan { get; set; }  // Catatan tambahan
        public List<InfeksiDetailViewModel>? Details { get; set; }
    }
}
