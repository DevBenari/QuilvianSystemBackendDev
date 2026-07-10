using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Finance.All.Models
{
    [Table("Fin_DetAccManualJurnal", Schema = "public")]
    public class AccManualJurnalDetail : UserActivity
    {
        [Key]
        public Guid DetAccManualJurnalId { get; set; }

        [Required]
        public Guid AccManualJurnalId { get; set; }

        [ForeignKey(nameof(AccManualJurnalId))]
        public AccManualJurnal? AccManualJurnal { get; set; }

        /// <summary>
        /// Boleh kosong jika detail tidak berasal dari recurring journal.
        /// </summary>
        public Guid? DetailTempRJId { get; set; }

        [ForeignKey(nameof(DetailTempRJId))]
        public RecurringJournalDetail? RecurringJournalDetail { get; set; }

        [Required]
        public Guid COAId { get; set; }

        [MaxLength(100)]
        public string? COACode { get; set; }

        [MaxLength(200)]
        public string? COAName { get; set; }

        /// <summary>
        /// Menjelaskan peran COA dalam jurnal.
        /// Contoh: Kas, Pendapatan, Beban, Piutang, atau Hutang.
        /// </summary>
        [MaxLength(200)]
        public string? RoleSetupCOA { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal DebetAmount { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal CreditAmount { get; set; }

        public Guid? KunjunganId { get; set; }

        [MaxLength(100)]
        public string? NoRegistrasi { get; set; }

        public Guid? CostCenterId { get; set; }

        [MaxLength(200)]
        public string? CostCenterName { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }
    }
}