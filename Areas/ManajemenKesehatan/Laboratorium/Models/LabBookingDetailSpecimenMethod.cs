using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models
{
        [Table("LabBookingDetailSpecimenMethod", Schema = "public")]
        public class LabBookingDetailSpecimenMethod : UserActivity
        {
            [Key]
            public Guid LabBookingDetailSpecimenMethodId { get; set; }

            public Guid? DetailBookingLabId { get; set; }

            public Guid? SpecimenMethodId { get; set; }

            public string? Keterangan { get; set; }
        }
    
}
