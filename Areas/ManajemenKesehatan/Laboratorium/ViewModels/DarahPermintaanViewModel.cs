namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels
{
    public class DarahPermintaanViewModel
    {
        public Guid? KomponenDarahId { get; set; }         // Relasi ke tabel komponen darah
        public Guid? GolonganDarahId { get; set; }         // Relasi ke tabel golongan darah

        public decimal? JumlahKantong { get; set; }        // Jumlah kantong darah yang diminta
        public bool? Rhesus { get; set; }                  // True = Positif, False = Negatif

        public string? TglPemesanan { get; set; }        // Tanggal pemesanan
        public TimeOnly? WaktuPemesanan { get; set; }      // Waktu pemesanan
        public string? TglDiperlukan { get; set; }       // Tanggal darah diperlukan
        public Guid? Petugas { get; set; } //petugas yg ambil darah
        public Guid? DokterPerujukId { get; set; }                // Dokter Perujuk
        public Guid? DokterBDRSId { get; set; }            // Dokter BDRS
        public string? Keterangan { get; set; }            // Catatan tambahan
    }
}
