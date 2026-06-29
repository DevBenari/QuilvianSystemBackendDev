using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Finance.AP.Models
{
    [Table("Fin_PembayaranAP", Schema = "public")]
    public class PembayaranAP : UserActivity
    {
        [Key]
        public Guid PembayaranAPId { get; set; }

        [MaxLength(100)]
        public string? KodePembayaranAP { get; set; }

        [MaxLength(100)]
        public string? NoReferensi { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? TotalTagihan { get; set; }

        public Guid? SupplierId { get; set; }

        // GET saja dari master supplier by SupplierId
        [NotMapped]
        public string? NamaSupplier { get; set; }

        // GET saja dari Purchasing Invoice / Detail Tukar Faktur
        [NotMapped]
        public string? NoTukarFaktur { get; set; }

        public DateTime? TglPembayaranAP { get; set; }

        public Guid? BankId { get; set; }

        // GET saja dari bank account by BankId
        [NotMapped]
        public string? NamaBank { get; set; }

        // GET saja dari Purchasing Invoice
        [NotMapped]
        public DateTime? TglJatuhTempo { get; set; }

        [MaxLength(100)]
        public string? TipePembayaran { get; set; }

        [MaxLength(100)]
        public string? StatusPembayaran { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? Potongan { get; set; }

        public string? ReceiveOrderNumber { get; set; }

        public decimal? DPPo { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }

        public List<DetailPembayaranAP>? DetailPembayaranAPs { get; set; }
    }
}