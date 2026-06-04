using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models
{
    public class LabPemeriksaan : UserActivity
    {
        [Key]
        public Guid PemeriksaanLabId { get; set; } // Generate Otomatis
        public string? NamaPemeriksaan { get; set; }
        public string? KodePemeriksaan { get; set; }
        public decimal? HargaPemeriksaan { get; set; } // Harga Pemeriksaan
        public decimal? DurasiPuasa { get; set; } 
        public bool? IsButuhPersiapan { get; set; } 
        public Guid? KategoriPemeriksaanId { get; set; } // Relasi ke tabel Kategori Pemeriksaan
        public string? Keterangan { get; set; } // Keterangan tambahan

        public LabKategoriPemeriksaan? KategoriPemeriksaan { get; set; }
    }
}
