using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.Finance.Retur.Models
{
    [Table("Fin_DepositRetur", Schema = "public")]
    public class DepositRetur : UserActivity
    {
        [Key]
        public Guid DepositReturId { get; set; } = Guid.NewGuid();

        public Guid PoId { get; set; }

        [NotMapped]
        public string? NoPO { get; set; }

        public Guid SupplierId { get; set; }

        [NotMapped]
        public string? NamaSupplier { get; set; }

        public Guid ReceiveOrderId { get; set; }

        [NotMapped]
        public string? ReceiveOrderNumber { get; set; }

        public Guid HeaderReturId { get; set; }

        [NotMapped]
        public string? KodeRetur { get; set; }

        public DateTime TglInsertDeposit { get; set; }

        [MaxLength(50)]
        public string? StatusDeposit { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal JumlahDeposit { get; set; }

        public string? Keterangan { get; set; }
    }
}
