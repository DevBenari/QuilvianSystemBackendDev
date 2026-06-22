
using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Finance.Po.Models
{
    [Table("Fin_PurchasingInvoice", Schema = "public")]
    public class PurchasingInvoice : UserActivity
    {
        [Key]
        public Guid PurchasingInvoiceId { get; set; }

        public Guid? POId { get; set; }

        public string? NoPO { get; set; }

        public DateTime? TglPO { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? POAmount { get; set; }

        public Guid? SupplierId { get; set; }

        public string? NamaSupplier { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiskonSupplier { get; set; }

        public int? SupplierTermPayment { get; set; }

        public DateTime? TglPembuatanInvoice { get; set; }

        public DateTime? TglJatuhTempo { get; set; }

        public string? TipePembayaran { get; set; }

        public Guid? ReceiveOrderId { get; set; }

        public string? ReceiveOrderNumber { get; set; }

        public string? NoInvoice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DownPayment { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiskonPersen { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiskonNominal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PPNPersen { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PPNNominal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OngkosKirim { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Materai { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Pembulatan { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Potongan { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Retur { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OutstandingDP { get; set; }

        public Guid? COAId { get; set; }

        public string? NoFakturPajak { get; set; }

        public DateTime? TglFaktur { get; set; }

        public Guid? MataUangId { get; set; }

        public string? NamaMataUang { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? RateToIdr { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? HasilKonversi { get; set; }

        public string? Keterangan { get; set; }

        public DateTime CreateDateTime { get; set; } = DateTime.UtcNow;

        public DateTime? UpdateDateTime { get; set; }

        public string NoTukarFaktur { get; set; } = string.Empty;
        public bool? IsDelete { get; set; } = false;
        public string? Status { get; set; }

        public ICollection<ItemPurchasingInvoice> Items { get; set; } = new List<ItemPurchasingInvoice>();
    }

}
