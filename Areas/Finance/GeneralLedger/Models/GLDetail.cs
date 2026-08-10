using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Finance.GeneralLedger.Models
{
    [Table("Fin_GLDetail", Schema = "public")]
    public class GLDetail : UserActivity
    {
        [Key]
        public Guid GLDetailId { get; set; }

        public Guid GLHeaderId { get; set; }

        public Guid COAId { get; set; }

        public Guid DetailTempRJId { get; set; }
        public string? RoleSetupCOA { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal NilaiDebit { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal NilaiKredit { get; set; }

        [MaxLength(100)]
        public string? SourceItemType { get; set; }

        [MaxLength(100)]
        public string? SourceId { get; set; }

        [MaxLength(100)]
        public string? SourceNumber { get; set; }

        public Guid? SourceItemId { get; set; }

        [MaxLength(250)]
        public string? SourceItem { get; set; }

        public Guid? CostCenterId { get; set; }

        [MaxLength(250)]
        public string? CostCenterName { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }
    }
}