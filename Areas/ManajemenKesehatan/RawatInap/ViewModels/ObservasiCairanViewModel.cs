namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels
{
    public class ObservasiCairanViewModel
    {
        public Guid? KunjunganId { get; set; }             // Relasi ke tabel Kunjungan
        public Guid? PasienId { get; set; }                // Relasi ke tabel pendaftaran pasien baru
        public Guid? UserActivePerawatId { get; set; }            // Id Perawat
        public DateTime? TglObservasi { get; set; }        // Tanggal Observasi
        public string? Intake { get; set; }
        public string? Outake { get; set; }
        public decimal? CairanMasuk { get; set; }           // Cairan masuk
        public decimal? CairanKeluar { get; set; }          // Cairan keluar
        public decimal? CairanSisa { get; set; }          // Cairan sisa
        public decimal? JumlahUrin { get; set; }          // Jumlah urin
        public string? Keterangan { get; set; }            // Catatan tambahan
    }
}
