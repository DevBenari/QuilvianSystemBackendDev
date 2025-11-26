namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels
{
    public class TindakanHarianViewModel
    {
        public List<Guid>? TindakanPerawatId { get; set; }
        public Guid? KunjunganId { get; set; }        // Relasi ke tabel kunjungan
        public Guid? PasienId { get; set; }           // Relasi ke tabel pendaftaran pasien baru
        public string? TglTindakanHarian { get; set; }   // Tanggal tindakan
        public TimeOnly? WaktuTindakanHarian { get; set; } // Jam tindakan
        public string? ShiftTime { get; set; }       // Pagi / Siang / Malam
        public string? Keterangan { get; set; }      // Catatan tambahan
        public string? NamaPerawat { get; set; }     // Nama perawat
        public string? Diagnosa { get; set; }
    }
}
