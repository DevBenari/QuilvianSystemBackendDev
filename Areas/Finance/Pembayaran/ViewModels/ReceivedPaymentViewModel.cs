using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.Finance.Pembayaran.ViewModels
{
    public class ReceivedPaymentViewModel
    {
        public Guid? BankId { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? TotalReceived { get; set; }

        public DateTime? TglPembayaran { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? SisaPembayaran { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? TotalTagihanPasien { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? PembayaranKe { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }
    }
}
