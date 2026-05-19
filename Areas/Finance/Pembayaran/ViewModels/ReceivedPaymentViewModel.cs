using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.Finance.Pembayaran.ViewModels
{
    public class ReceivedPaymentViewModel
    {
        public Guid? BankId { get; set; }

        public string? NamaPasien { get; set; }

        public string? NoRm { get; set; }

        public string? NamaPerusahaan { get; set; }

        public string? NamaAsuransi { get; set; }

        public string? NoInvoice { get; set; }

        public decimal TotalSaldoAwal { get; set; }

        public Guid DetailReceivedPaymentId { get; set; }

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
