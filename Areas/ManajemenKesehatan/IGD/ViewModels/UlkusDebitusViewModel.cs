namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.ViewModels
{
    public class UlkusDebitusViewModel
    {
        public Guid? KunjunganId { get; set; }      // Relasi ke tabel Kunjungan (Unit/Kamar)
        public Guid? PasienId { get; set; }         // Relasi ke tabel Pasien
        public DateTime? TglAwalTirahBaring { get; set; }    // Tanggal awal pasien tirah baring
        public DateTime? TglAkhirTirahBaring { get; set; }   // Tanggal akhir tirah baring
        public DateTime? TglDekubitus { get; set; }          // Tanggal kejadian dekubitus
        public string? AsalDekubitus { get; set; }           // Asal atau penyebab dekubitus
        public string? NamaTempatDekubitus { get; set; }     // Nama bagian tubuh yang terkena
        public Guid? IndicatorPengkajianId { get; set; }          // Relasi ke tabel skor indikator (misalnya Braden Score)
        public DateTime? TglPencatatan { get; set; }         // Waktu pencatatan form
        public string? LokasiUlkusDekubitus { get; set; }    // Lokasi luka dekubitus
        public string? Keterangan { get; set; }              // Catatan tambahan
    }
}
