using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.Models
{
    [Table("Hrd_MstLevel", Schema = "public")]
    public class Level
    {
        [Key]
        public Guid LevelId { get; set; }
        public string KodeLevel { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MinSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MaxSalary { get; set; }

        public string Keterangan { get; set; }
    }
}
