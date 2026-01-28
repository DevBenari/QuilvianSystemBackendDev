using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Finance.Po.Models
{
    [Table("PurchaseOrder", Schema = "public")]
    public class PurchaseOrder : UserActivity
    {
        [Key]
        public Guid PurchaseOrderId { get; set; }

        public string PurchaseOrderNumber { get; set; }
        public Guid PurchaseRequestId { get; set; }
        public string PurchaseRequestNumber { get; set; }
        public string RequestType { get; set; }

        public Guid SupplierId { get; set; }
        public Guid TermOfPaymentId { get; set; }

        public DateTime ExpiredDate { get; set; }
        public decimal RemainingDay { get; set; }
        public decimal QtyTotal { get; set; }
        public decimal GrandTotal { get; set; }
        public string Keterangan { get; set; }

    }
}
