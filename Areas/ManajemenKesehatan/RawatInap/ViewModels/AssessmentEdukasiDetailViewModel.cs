namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels
{
    public class AssessmentEdukasiDetailViewModel
    {
        public Guid? AsesmenEdukasiId { get; set; }       // Relasi ke tabel AsesmenEdukasi
        public Guid? TopikEdukasiId { get; set; }         // Topik edukasi yang diberikan
        public DateTime? TglDetailAsesmenEdukasi { get; set; } // Tanggal asesmen dilakukan
        public decimal? DurasiWaktu { get; set; }         // Lama edukasi (dalam menit/jam)
        public string? NamaWali { get; set; }             // Nama wali pasien (jika ada)

        // 🔹 File Upload untuk tanda tangan wali
        public IFormFile? TTDWali { get; set; }           // File tanda tangan wali

        public string? TingkatPemahaman { get; set; }     // Pemahaman pasien/wali terhadap edukasi
        public string? MetodeEdukasi { get; set; }        // Metode edukasi (lisan, tulisan, demonstrasi)
        public string? SaranaEdukasi { get; set; }        // Sarana yang digunakan (poster, video, leaflet)

        // 🔹 File Upload untuk tanda tangan perawat
        public Guid? TTDPerawatId { get; set; }        // File tanda tangan perawat

        public string? EvaluasiEdukasi { get; set; }      // Evaluasi hasil edukasi
        public string? Keterangan { get; set; }           // Catatan tambahan
        public DateTime? TglEvaluasiEdukasi { get; set; } // Tanggal evaluasi dilakukan
    }
}
