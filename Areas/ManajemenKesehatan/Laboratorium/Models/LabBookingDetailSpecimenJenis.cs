using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models
{
    [Table("LabBookingDetailSpecimenJenis", Schema = "public")]
    public class LabBookingDetailSpecimenJenis : UserActivity
    {
        [Key]
        public Guid LabBookingDetailSpecimenJenisId { get; set; }

        public Guid? DetailBookingLabId { get; set; }

        public Guid? SpecimenJenisId { get; set; }

        public string? Keterangan { get; set; }
    }
}
