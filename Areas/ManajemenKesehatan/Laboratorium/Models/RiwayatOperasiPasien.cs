using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models
{
    public class RiwayatOperasiPasien : UserActivity
    {
        [Key]
        public Guid RiwayatOperasiPasienId { get; set; }

        public Guid? KunjunganId { get; set; }

        public Guid? PasienId { get; set; }

        // Bisa berasal dari LabId / KunjunganId / RadiologiId / dll
        public Guid? SumberDataId { get; set; }

        // Radiologi / Rajal / Ranap / IGD / Laboratorium / dll
        public string? NamaSumberData { get; set; }

        public string? NamaOperasi { get; set; }

        // Operasi dilakukan di bagian tubuh mana
        public string? LokasiTubuh { get; set; }

        // Indikasi / pernyataan dari pasien kenapa dioperasi
        public string? IndikasiOperasi { get; set; }

        public DateTime? WaktuOperasi { get; set; }

        public string? Keterangan { get; set; }


        // =========================
        // Navigation Property
        // =========================

        [ForeignKey(nameof(KunjunganId))]
        public virtual Kunjungan? Kunjungan { get; set; }

        [ForeignKey(nameof(PasienId))]
        public virtual PendaftaranPasienBaru? Pasien { get; set; }

        // SumberDataId tidak dibuat navigation langsung,
        // karena sumbernya bisa berasal dari banyak tabel.
    }
}
