using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Finance.Po.Models
{
    [Table("Fin_PurchaseOrder", Schema = "public")]
    public class PurchaseOrder : UserActivity
    {
        [Key]
        public Guid PurchaseOrderId { get; set; }

        public string? PurchaseRequestNumber { get; set; }
        public string? PurchaseOrderNumber { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string? InvoiceNumber { get; set; }
        public string? RequestType { get; set; }
        public string? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public string? SupplierCode { get; set; }
        public string? TermOfPayment { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public decimal? RemainingDay { get; set; }
        public decimal? QtyTotal { get; set; }
        public decimal? GrandTotal { get; set; }

        public string? UserAccess { get; set; }
        public string? StatusPO { get; set; }
        public string? Keterangan { get; set; }

        // Navigation Property
        public ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; }

    }
}
