using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.Models
{
    [Table("Hrd_MstGradePay", Schema = "public")]
    public class GradePay
    {
        [Key]
        public Guid GradePayId { get; set; }
        public string KodeGrade { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MinSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MaxSalary { get; set; }

        public string Keterangan { get; set; }
    }
}
