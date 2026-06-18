using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Finance.Po.Models
{
    [Table("Fin_ItemPurchasingInvoice", Schema = "public")]
    public class ItemPurchasingInvoice : UserActivity
    {
        [Key]
        public Guid ItemPurchasingInvoiceId { get; set; }

        public Guid PurchasingInvoiceId { get; set; }

        public Guid? POId { get; set; }

        public Guid? ItemPOId { get; set; }

        public string? KodeProduk { get; set; }

        public string? NamaProduk { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? QtyProduk { get; set; }

        public string? SatuanProduk { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? HargaNormal { get; set; }

        public string? TipeTax { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PajakPersen { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PajakNominal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? HargaAkhir { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? HargaTotal { get; set; }

        public string? Keterangan { get; set; }

        public DateTime CreateDateTime { get; set; } = DateTime.UtcNow;

        public DateTime? UpdateDateTime { get; set; }

        public bool? IsDelete { get; set; } = false;

        [ForeignKey(nameof(PurchasingInvoiceId))]
        public PurchasingInvoice? PurchasingInvoice { get; set; }
    }

}
