using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.Models
{
    public class SubLevel : UserActivity
    {
        [Key]
        public Guid SubLevelId { get; set; }

        public Guid LevelId { get; set; }

        public float SubLevelNum { get; set; }

        public Guid PayGrade { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BasicSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AdditionalSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subsidy { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Compensation { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Reimbursement { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DailyTransport { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MealAllowance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MealOutsideOffice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiligentFee { get; set; }

        public bool isOvertime { get; set; }
        public bool isAbsent { get; set; }
        public bool isInsentif { get; set; }
        public bool isBonus { get; set; }
        public bool isLeaveCompansation { get; set; }
        public bool isPositionAllowance { get; set; }

        public string? Keterangan { get; set; }

    }
}
