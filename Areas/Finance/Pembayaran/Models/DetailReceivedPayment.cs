using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.Finance.Pembayaran.Models
{
    [Table("Fin_DetailReceivedPayment", Schema = "public")]
    public class DetailReceivedPayment : UserActivity
    {
        [Key]
        public Guid DetailReceivedPaymentId { get; set; }

        public Guid? ReceivedPaymentId { get; set; }

        public Guid? AsuransiId { get; set; }

        [MaxLength(100)]
        public string? NoInvoice { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? TotalInvoice { get; set; }

        public DateTime? DueDate { get; set; }

        public bool? IsCanceled { get; set; }

        public Guid? COADiskonId { get; set; }

        [MaxLength(200)]
        public string? NamaCOADiskon { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? PersenCOADiskon { get; set; }

        public Guid? COATambahanId { get; set; }

        [MaxLength(200)]
        public string? NamaCoaTambahan { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? NominalTambahan { get; set; }
        [Column(TypeName = "numeric")]
        public decimal? PPH23Per { get; set; }
        [Column(TypeName = "numeric")]
        public decimal? PPH23Nom { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }
    }
}
