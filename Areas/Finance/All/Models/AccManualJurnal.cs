using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Finance.All.Models
{
    [Table("Fin_AccManualJurnal", Schema = "public")]
    public class AccManualJurnal : UserActivity
    {
        [Key]
        public Guid AccManualJurnalId { get; set; }

        /// <summary>
        /// Dibuat otomatis dengan format ACC-MJ-00001.
        /// Nomor bertambah setiap pembuatan jurnal baru.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string KodeManualJurnal { get; set; } = string.Empty;

        public DateTime TglDokumen { get; set; }

        /// <summary>
        /// Tanggal transaksi jurnal manual.
        /// </summary>
        public DateTime TglManualJurnal { get; set; }

        /// <summary>
        /// Diisi ketika jurnal dibatalkan.
        /// </summary>
        public DateTime? TglPembatalan { get; set; }

        [Required]
        [MaxLength(100)]
        public string TipeDokumen { get; set; } = string.Empty;

        /// <summary>
        /// Boleh kosong jika jurnal tidak dibuat dari recurring journal.
        /// </summary>
        public Guid? TempRJId { get; set; }

        [ForeignKey(nameof(TempRJId))]
        public RecurringJournal? RecurringJournal { get; set; }

        [MaxLength(200)]
        public string? RecurringJournalName { get; set; }

        public DateTime? RecurringJournalDate { get; set; }

        [Required]
        public Guid MataUangId { get; set; }

        [MaxLength(100)]
        public string? NamaMataUang { get; set; }

        [Required]
        public Guid ExchangeRateId { get; set; }

        [Column(TypeName = "numeric(18,6)")]
        public decimal RateToIdr { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal UnbalancedAmount { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }

        public List<AccManualJurnalDetail>? AccManualJurnalDetails { get; set; }
    }
}