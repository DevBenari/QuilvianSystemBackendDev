using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;
using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models
{
    public class LabBakteriDetail : UserActivity
    {
        [Key]
        public Guid LabDetailBakteriId { get; set; }

        public Guid? LabHasilBakteriId { get; set; }

        public Guid? KunjunganId { get; set; }

        public Guid? PasienId { get; set; }
        
        public Guid AntibiotikId { get; set; }

        public string? RangeZona { get; set; }

        public decimal? ZonaMM { get; set; }

        public string? ResultAntibiotik { get; set; }

        public string? Keterangan { get; set; }


        // Navigation
        public LabHasilBakteri? LabHasilBakteri { get; set; }

        public Kunjungan? Kunjungan { get; set; }

        public PendaftaranPasienBaru? Pasien { get; set; }

        public MstAntibiotik? Antibiotik { get; set; }
    }
}
