using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.Finance.Faktur.ViewModels
{
    public class TukarFakturViewModel
    {

        public Guid SupplierId { get; set; }

        // Kalau NamaSupplier hanya diambil dari master supplier, lebih aman NotMapped
        // supaya tidak wajib jadi kolom di table FIN_TukarFaktur
        [NotMapped]
        public string? NamaSupplier { get; set; }

        [Required]
        [MaxLength(50)]
        public string NoTukarFaktur { get; set; } = string.Empty;

        public DateTime TglRegistrasi { get; set; }

        public DateTime? TglTerimaFaktur { get; set; }

        public DateTime? TglJatuhTempo { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal TotalInvoiceGRN { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal TotalInvoiceAP { get; set; }

        public string? Keterangan { get; set; }
    }
}
