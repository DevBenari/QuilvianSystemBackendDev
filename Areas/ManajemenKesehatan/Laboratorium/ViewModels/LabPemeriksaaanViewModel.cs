namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels
{
    public class LabPemeriksaaanViewModel
    {
        public string? NamaPemeriksaan { get; set; }
        public string? KodePemeriksaan { get; set; }
        public decimal? HargaPemeriksaan { get; set; } // Harga Pemeriksaan
        public Guid? KategoriPemeriksaanId { get; set; } // Relasi ke tabel Kategori Pemeriksaan
        public string? Keterangan { get; set; } // Keterangan tambahan
    }
}
