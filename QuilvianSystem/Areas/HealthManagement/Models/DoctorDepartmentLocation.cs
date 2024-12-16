using QuilvianSystem.Areas.Administration.Models;
using QuilvianSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystem.Areas.HealthManagement.Models
{
    [Table("HmnDepartmentLocation", Schema = "dbo")]
    public class DoctorDepartmentLocation : UserActivity
    {
        [Key]
        public Guid LocationId { get; set; }
        public string KodeLokasi { get; set; }
        public string NamaLokasi { get; set; }
        public string? Keterangan { get; set; }
        public ICollection<DoctorDepartment> DoctorDepartments { get; set; }        
    }
}
