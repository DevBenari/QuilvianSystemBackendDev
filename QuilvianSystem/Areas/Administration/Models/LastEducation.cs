
using QuilvianSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystem.Areas.Administration.Models
{
    [Table("AdmLastEducation", Schema = "dbo")]
    public class LastEducation : UserActivity //UserActivity Create on Repository
    {
        [Key]
        public Guid LastEducationId { get; set; }
        public string KodePendidikanTerakhir { get; set; }
        public string NamaPendidikanTerakhir { get; set; }
    }
}
