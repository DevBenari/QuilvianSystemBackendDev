namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels
{
    public class ObservasiCairanViewModel
    {
        public Guid? KunjunganId { get; set; }             // Relasi ke tabel Kunjungan
        public Guid? PasienId { get; set; }                // Relasi ke tabel pendaftaran pasien baru
        public Guid? UserActivePerawatId { get; set; }            // Id Perawat
        public string? CairanMasuk { get; set; }           // Cairan masuk
        public string? CairanKeluar { get; set; }          // Cairan keluar
        public decimal? CairanSisa { get; set; }          // Cairan sisa
        public decimal? JumlahUrin { get; set; }          // Jumlah urin
        public IFormFile? TTDFile { get; set; }
        public string? Keterangan { get; set; }            // Catatan tambahan
    }
}
