namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels
{
    public class PengkajianEliminasiViewModel
    {
        public Guid? KunjunganId { get; set; }          // Relasi dengan tabel Kunjungan
        public Guid? PengkajianPerawatId { get; set; }  // Relasi dengan tabel Pengkajian Perawat
        public string? MasalahPerkemihan { get; set; }  // BAK
        public string? MasalahDefekasi { get; set; }    // BAB
        public string? WarnaBAK { get; set; }           // Warna urin
        public string? AlatBantuEliminasi { get; set; } // Contoh: kateter, pispot, dll
        public string? JenisKateter { get; set; }       // Jenis kateter
        public string? UkuranKateter { get; set; }      // Ukuran kateter
        public string? Keterangan { get; set; }
    }
}
