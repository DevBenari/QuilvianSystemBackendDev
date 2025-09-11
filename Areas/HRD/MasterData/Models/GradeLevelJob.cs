using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.Models
{
    [Table("Hrd_MstGradeLevelJob", Schema = "public")]
    public class GradeLevelJob : UserActivity
    {
        [Key]
        public Guid GradeLevelJobId { get; set; }

        //public Guid GradeLevelId { get; set; } // tidak digunakan
        public Guid PositionId { get; set; }
        public Guid GradeId { get; set; }
        public Guid LevelId { get; set; }

        public string? Keterangan { get; set; }
    }
}
