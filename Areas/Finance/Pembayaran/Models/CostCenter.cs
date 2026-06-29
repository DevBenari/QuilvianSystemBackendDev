using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Finance.Pembayaran.Models
{
    [Table("Fin_CostCenter", Schema = "public")]
    public class CostCenter : UserActivity
    {
        [Key]
        public Guid CostCenterId { get; set; } = Guid.NewGuid();

        [MaxLength(100)]
        public string? KodeCostCenter { get; set; }

        [MaxLength(250)]
        public string? LokasiCostCenter { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }
    }
}