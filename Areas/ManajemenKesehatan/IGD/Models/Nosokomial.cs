using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.Models
{
    public class Nosokomial : UserActivity
    {
        [Key]
        public Guid NosokomialId { get; set; } // Generate Otomatis
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public decimal? TB { get; set; } // Tinggi Badan (cm)
        public decimal? BB { get; set; } // Berat Badan (kg)
        public string? CaraMasukRS { get; set; } // IGD, Rawat Jalan, Rujukan, dll
        public DateTime? TglMasukRs { get; set; } // Tanggal masuk RS
        public DateTime? TglKeluarRs { get; set; } // Tanggal keluar RS
        public Guid? DokterId1 { get; set; }
        public Guid? DokterId2 { get; set; }
        public Guid? DokterId3 { get; set; }
        public Guid? IPCLN1 { get; set; } // Perawat ID
        public Guid? IPCLN2 { get; set; } // Perawat ID
        public Guid? IPCLN3 { get; set; } // Perawat ID
        public string? KondisiKeluar { get; set; } // Hidup / Meninggal / Pindah RS
        public string? DiagnosaAwal { get; set; }
        public string? DiagnosaAkhir { get; set; }
        public string? TTDKepalaRuangan { get; set; } // File path / Base64 signature
        public Guid? KepalaRuanganId { get; set; }
        public string? TTDPerawat { get; set; } // File path / Base64 signature
        public Guid? PerawatId { get; set; }

    }
}
