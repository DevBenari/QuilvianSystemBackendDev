using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Finance.GeneralLedger.Models
{
    [Table("Fin_GLHeader", Schema = "public")]
    public class GLHeader : UserActivity
    {
        [Key]
        public Guid GLHeaderId { get; set; }

        [MaxLength(50)]
        public string? GLKode { get; set; }

        public Guid KunjunganId { get; set; }

        [MaxLength(100)]
        public string? NoRegistrasi { get; set; }

        [MaxLength(100)]
        public string? JenisKunjungan { get; set; }

        public Guid PasienId { get; set; }

        public DateTime TglTransaksi { get; set; }

        public DateTime TglPosting { get; set; }

        [MaxLength(100)]
        public string? SourceGL { get; set; }

        [MaxLength(100)]
        public string? SourceTypeGL { get; set; }

        public Guid SourceId { get; set; }

        [MaxLength(100)]
        public string? SourceNumber { get; set; }

        [MaxLength(20)]
        public string? GLStatus { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }


        // ==========================================
        // RECURRING JOURNAL
        // ==========================================

        public Guid? TempRJId { get; set; }


        // ==========================================
        // CURRENCY
        // ==========================================

        public Guid? MataUangId { get; set; }

        [MaxLength(100)]
        public string? NamaMataUang { get; set; }


        // ==========================================
        // EXCHANGE RATE
        // ==========================================

        public Guid? ExchangeRateId { get; set; }

        [Column(TypeName = "numeric(18,6)")]
        public decimal? RateToIdr { get; set; }


        // ==========================================
        // BALANCE
        // ==========================================

        [Column(TypeName = "numeric(18,2)")]
        public decimal? UnbalanceAmount { get; set; }
    }
}