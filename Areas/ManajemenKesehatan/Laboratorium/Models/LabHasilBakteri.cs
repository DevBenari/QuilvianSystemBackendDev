using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;
using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models
{
    public class LabHasilBakteri : UserActivity
    {
        [Key]
        public Guid LabHasilBakteriId { get; set; }

        public Guid? LabHasilId { get; set; }

        public Guid? KunjunganId { get; set; }

        public Guid? PasienId { get; set; }

        public Guid? LabBookingId { get; set; }

        public Guid? MappingBakteriId { get; set; }

        public string? Keterangan { get; set; }


        // Navigation
        public LabHasil? LabHasil { get; set; }

        public Kunjungan? Kunjungan { get; set; }

        public PendaftaranPasienBaru? Pasien { get; set; }

        public LabBooking? LabBooking { get; set; }

        public MstMappingBakteri? MappingBakteri { get; set; }


        // Detail
        public ICollection<LabBakteriDetail> LabDetailBakteris { get; set; }
            = new List<LabBakteriDetail>();
    }
}
