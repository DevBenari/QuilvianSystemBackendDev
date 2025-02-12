using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstDepartment", Schema = "dbo")]
    public class Department : UserActivity
    {
        [Key]
        public Guid DepartmentId { get; set; }
        public string DepartmentCode { get; set; }
        public string DepartmentName { get; set; }
        public string? Note { get; set; }
        public ICollection<Position> Positions { get; set; }
    }

    [Table("MstPosition", Schema = "dbo")]
    public class Position : UserActivity
    {
        [Key]
        public Guid PositionId { get; set; }
        public string PositionCode { get; set; }
        public string PositionName { get; set; }
        public Guid? DepartmentId { get; set; }

        //Relationship
        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }
    }
}
