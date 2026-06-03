using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.Finance.Pembayaran.Models
{
    [Table("Fin_ReceivedPayment", Schema = "public")]
    public class ReceivedPayment : UserActivity
    {
        [Key]
        public Guid ReceivedPaymentId { get; set; }

        public Guid? BankId { get; set; }
        public Guid? AyatSilangId { get; set; }
        public string? NoInvoice { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? TotalReceived { get; set; }

        public DateTime? TglPembayaran { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? SisaPembayaran { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? TotalTagihanPasien { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? PembayaranKe { get; set; }
        public bool? IsCanceled { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }
    }
}
