namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.ViewModels
{
    public class InfeksiSKViewModel
    {
        public Guid? KunjunganId { get; set; } // Relasi ke tabel kunjungan / Unit / Kamar
        public Guid? PasienId { get; set; } // Relasi ke tabel pendaftaran pasien
        public string? KateterUrin { get; set; } // Jenis atau nomor kateter urin
        public DateTime? TglLeukositUrin1 { get; set; } // Tanggal leukosit urin pertama
        public DateTime? TglLeukositUrin2 { get; set; } // Tanggal leukosit urin kedua
        public DateTime? TglBiakanUrin1 { get; set; } // Tanggal biakan urin pertama
        public DateTime? TglBiakanUrin2 { get; set; } // Tanggal biakan urin kedua
        public string? HasilBiakanUrin1 { get; set; } // Hasil pemeriksaan biakan urin pertama
        public string? HasilBiakanUrin2 { get; set; } // Hasil pemeriksaan biakan urin kedua
        public DateTime? TglPencatatan { get; set; } // Waktu pencatatan form
        public string? Keterangan { get; set; } // Catatan tambahan
    }
}
