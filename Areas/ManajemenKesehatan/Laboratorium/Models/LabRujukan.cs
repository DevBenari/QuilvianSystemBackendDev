using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models
{
    public class LabRujukan : UserActivity
    {
        [Key]
        public Guid LabRujukanId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public Guid? LabId { get; set; }
        public string? ArahRujukan { get; set; }
        public Guid? FaskesRujukanId { get; set; }
        public string? DokterPerujuk { get; set; }
        public string? TglRujukan { get; set; }
        public string? Keterangan { get; set; }

        [ForeignKey(nameof(KunjunganId))]
        public virtual Kunjungan? Kunjungan { get; set; }

        [ForeignKey(nameof(PasienId))]
        public virtual PendaftaranPasienBaru? Pasien { get; set; }

        [ForeignKey(nameof(LabId))]
        public virtual Lab? Lab { get; set; }

        [ForeignKey(nameof(FaskesRujukanId))]
        public virtual FaskesRujukan? FaskesRujukan { get; set; }
    }
}
