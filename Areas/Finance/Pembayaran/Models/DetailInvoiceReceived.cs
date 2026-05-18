using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.Finance.Pembayaran.Models
{
    [Table("Fin_DetailInvoiceReceived", Schema = "public")]
    public class DetailInvoiceReceived : UserActivity
    {
        [Key]
        public Guid DetailInvoicePaymentId { get; set; }

        public Guid? DetailReceivedPaymentId { get; set; }

        public Guid? KunjunganId { get; set; }

        public Guid? PasiemId { get; set; }

        [MaxLength(50)]
        public string? NoRM { get; set; }

        [MaxLength(200)]
        public string? NamaPasien { get; set; }

        [MaxLength(100)]
        public string? NoBilling { get; set; }

        public DateTime? TglTerima { get; set; }

        public DateTime? TglKirim { get; set; }

        public DateTime? TglTagihan { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? TotalPiutang { get; set; }
        public decimal? PiutangTerbayar { get; set; }
        public int? PembayaranKe { get; set; }
        public string? ApprovedVp { get; set; }
        public string? FileBuktiPembayaran { get; set; }
        public DateTime? TglJaatuhTempo { get; set; }

        public bool? IsTerbayar { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }
    }
}
