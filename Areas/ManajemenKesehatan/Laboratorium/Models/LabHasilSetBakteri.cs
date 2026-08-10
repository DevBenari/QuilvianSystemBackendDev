using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;
using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models
{
    public class LabHasilSetBakteri : UserActivity
    {
        [Key]
        public Guid LabHasilSetBakteriId { get; set; }

        public Guid? LabHasilId { get; set; }

        public Guid? KunjunganId { get; set; }

        public Guid? PasienId { get; set; }

        public Guid? AsalSpecimenId { get; set; }

        public Guid? JenisSpecimenId { get; set; }

        public string? Keterangan { get; set; }


        // =========================
        // Navigation Property
        // =========================
        public LabHasil? LabHasil { get; set; }

        public Kunjungan? Kunjungan { get; set; }

        public PendaftaranPasienBaru? Pasien { get; set; }

        public SpecimenAsal? AsalSpecimen { get; set; }

        public SpecimenJenis? JenisSpecimen { get; set; }
    }
}
