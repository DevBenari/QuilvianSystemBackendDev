using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.Finance.Pembayaran.ViewModels
{
    public class DetailDokumenReceivedViewModel
    {
        public Guid? DetailReceivedPaymentId { get; set; }

        public Guid? KunjunganId { get; set; }

        public Guid? PasienId { get; set; }

        [MaxLength(100)]
        public string? NoBilling { get; set; }

        [MaxLength(255)]
        public string? SuratPengantar { get; set; }

        [MaxLength(255)]
        public string? Kwitansi { get; set; }

        [MaxLength(255)]
        public string? RekapitulasiTagihan { get; set; }

        [MaxLength(255)]
        public string? Invoice { get; set; }

        [MaxLength(255)]
        public string? TandaTerima { get; set; }

        public DateTime? TglTerima { get; set; }

        public DateTime? TglKirim { get; set; }

        public DateTime? TglTagihan { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? TotalPiutang { get; set; }

        public DateTime? TglJaatuhTempo { get; set; }

        public bool? IsTerbayar { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }
    }
}
