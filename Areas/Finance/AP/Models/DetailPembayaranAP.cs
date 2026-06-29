using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Finance.AP.Models
{
    [Table("Fin_DetailPembayaranAP", Schema = "public")]
    public class DetailPembayaranAP : UserActivity
    {
        [Key]
        public Guid DetailPembayaranAPId { get; set; }

        public Guid? PembayaranAPId { get; set; }

        public Guid? PurchasingInvoiceId { get; set; }

        // GET data dari table purchasing invoice by PurchasingInvoiceId
        [NotMapped]
        public string? KodePurchasingInvoice { get; set; }

        // GET data dari table purchasing invoice by PurchasingInvoiceId
        [NotMapped]
        public DateTime? TglPembuatanInvoice { get; set; }

        // GET data dari table purchasing invoice by PurchasingInvoiceId
        [NotMapped]
        public string? NoInvoice { get; set; }

        // GET data dari table purchasing invoice by PurchasingInvoiceId
        [NotMapped]
        public string? NoTukarFaktur { get; set; }

        // GET data dari table purchasing invoice by PurchasingInvoiceId
        [NotMapped]
        [Column(TypeName = "numeric")]
        public decimal? TotalTagihan { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? SisaTagihan { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? PembayaranTagihan { get; set; }

        //tambahan

        public string? ReceiveOrderNumber { get; set; }


        [Column(TypeName = "numeric(18,2)")]
        public decimal DPPo { get; set; }

        //end

        [MaxLength(500)]
        public string? Keterangan { get; set; }

        [ForeignKey(nameof(PembayaranAPId))]
        public PembayaranAP? PembayaranAP { get; set; }
    }
}