using QuilvianSystem.Areas.Administration.Models;
using QuilvianSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystem.Areas.HealthManagement.Models
{
    [Table("HmnMultipleDoctorDepartment", Schema = "dbo")]
    public class MultipleDoctorDepartment : UserActivity
    {
        [Key]
        public Guid MultipleDoctorDepartmentId { get; set; }
        public Guid? DoctorId { get; set; }
        public Guid? DoctorDepartmentId { get; set; }

        //Relationship
        [ForeignKey("DoctorId")]
        public Doctor? Doctor { get; set; }
        [ForeignKey("DoctorDepartmentId")]
        public DoctorDepartment? DoctorDepartment { get; set; }
    }
}
