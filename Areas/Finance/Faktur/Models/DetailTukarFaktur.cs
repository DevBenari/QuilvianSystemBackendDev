using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Finance.Faktur.Models
{
    [Table("Fin_DetailTukarFaktur", Schema = "public")]
    public class DetailTukarFaktur : UserActivity
    {
        [Key]
        public Guid DetailTukarFakturId { get; set; }

        public Guid TukarFakturId { get; set; }

        [NotMapped]
        public string? NoTukarFaktur { get; set; }

        public DateTime TglPembuatanInvoice { get; set; }

        [Required]
        [MaxLength(50)]
        public string KodePurchasingInvoice { get; set; } = string.Empty;

        public Guid POId { get; set; }

        public Guid SupplierId { get; set; }

        [NotMapped]
        public string? NamaSupplier { get; set; }

        [MaxLength(100)]
        public string NomorPO { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string NoInvoice { get; set; } = string.Empty;

        [Column(TypeName = "numeric(18,2)")]
        public decimal NilaiPurchasingInvoice { get; set; }

        [NotMapped]
        public DateTime? TglJatuhTempo { get; set; }

        [MaxLength(50)]
        public string StatusInvoice { get; set; } = "approved";

        public string? Keterangan { get; set; }
    }
}