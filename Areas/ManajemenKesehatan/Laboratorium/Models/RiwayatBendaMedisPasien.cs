using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models
{
    public class RiwayatBendaMedisPasien : UserActivity
    {
        [Key]
        public Guid RiwayatBendaMedisPasienId { get; set; }

        public Guid? KunjunganId { get; set; }

        public Guid? PasienId { get; set; }

        // Bisa berasal dari LabId / KunjunganId / RadiologiId / dll
        public Guid? SumberDataId { get; set; }

        // Radiologi / Rajal / Ranap / IGD / Laboratorium / dll
        public string? NamaSumberData { get; set; }

        public string? NamaBendaMedis { get; set; }

        // Benda medis berada di bagian tubuh mana
        public string? LokasiBendaMedis { get; set; }

        public bool? IsPermanen { get; set; }

        public string? Keterangan { get; set; }

        // =========================
        // Navigation Property
        // =========================

        [ForeignKey(nameof(KunjunganId))]
        public virtual Kunjungan? Kunjungan { get; set; }

        [ForeignKey(nameof(PasienId))]
        public virtual PendaftaranPasienBaru? Pasien { get; set; }

        // SumberDataId tidak dibuat navigation langsung,
        // karena sumbernya bisa banyak tabel: Lab, Kunjungan, Radiologi, IGD, dll.
    }
}
