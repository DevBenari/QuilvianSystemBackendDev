using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Finance.Po.Models
{
    [Table("PurchaseOrderItem", Schema = "public")]
    public class PurchaseOrderItem : UserActivity
    {
        [Key]
        public Guid PurchaseOrderItemId { get; set; }

        public Guid PurchaseOrderId { get; set; }
        public Guid ListPurchaseRequestId { get; set; }
        public Guid ProductId { get; set; }

        public string ProductName { get; set; }
        public string Measurement { get; set; }
        public string Category { get; set; }
        public string Layanan { get; set; }
        public string JenisPermintaan { get; set; }

        public decimal Qty { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public decimal SubTotal { get; set; }
        public string Keterangan { get; set; }

    }
}
