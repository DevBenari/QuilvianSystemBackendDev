using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.Finance.AP.Models
{

    [Table("Fin_ReceiveOrder", Schema = "public")]
    public class ReceiveOrder : UserActivity
    {
        [Key]
        public Guid ReceiveOrderId { get; set; }

        [MaxLength(100)]
        public string? ReceiveOrderNumber { get; set; }

        public Guid? PurchaseOrderId { get; set; }

        [MaxLength(100)]
        public string? InvoiceNumber { get; set; }

        public bool? IsInvoiceProvided { get; set; }

        [MaxLength(100)]
        public string? DeliveryNumber { get; set; }

        public DateTime? DueDate { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? TermOfPayment { get; set; }

        public Guid? SupplierId { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? StampDuty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? AdditionalDiscountRp { get; set; }

        [MaxLength(100)]
        public string? Status { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }

        public List<ReceiveOrderItem>? ReceiveOrderItems { get; set; }
    }
}
