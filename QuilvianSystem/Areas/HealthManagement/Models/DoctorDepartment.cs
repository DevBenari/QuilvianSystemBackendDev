using QuilvianSystem.Areas.Administration.Models;
using QuilvianSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystem.Areas.HealthManagement.Models
{
    [Table("HmnDoctorDepartment", Schema = "dbo")]
    public class DoctorDepartment : UserActivity
    {
        [Key]
        public Guid DepartmentId { get; set; }
        public string KodeDepartemen { get; set; }
        public string NamaDepartemen { get; set; }
        public Guid? LocationId { get; set; }
        public string? Telepon { get; set; }
        public string MulaiJamKerja { get; set; }
        public string SelesaiJamKerja { get; set; }
        public string? Keterangan { get; set; }
        public ICollection<MultipleDoctorDepartment> MultipleDoctorDepartment { get; set; }

        //Relationship
        [ForeignKey("LocationId")]
        public DoctorDepartmentLocation? DepartmentLocation { get; set; }
    }
}
