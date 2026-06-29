using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Finance.Po.Models
{
    [Table("Fin_PurchaseOrderItem", Schema = "public")]
    public class PurchaseOrderItem : UserActivity
    {
        [Key]
        public Guid PurchaseOrderItemId { get; set; }

        [ForeignKey("PurchaseOrder")]
        public Guid PurchaseOrderId { get; set; }

        public string? ProductName { get; set; }
        public string? Measurement { get; set; }
        public string? Category { get; set; }
        public string? Layanan { get; set; }
        public string? JenisPermintaan { get; set; }

        public decimal? Qty { get; set; }
        public decimal? Price { get; set; }
        public decimal? Discount { get; set; }
        public decimal? SubTotal { get; set; }

        // app.clickup.com/t/86ey1c1jk produkid di pindah ke child dari head
        public Guid? ProdukId { get; set; }
        public string? Keterangan { get; set; }

        // Navigation Property
        public PurchaseOrder PurchaseOrder { get; set; }

    }
}
