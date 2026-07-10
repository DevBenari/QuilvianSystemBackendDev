using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Finance.All.Models
{
    [Table("Fin_DetailTempRecurringJournal", Schema = "public")]
    public class RecurringJournalDetail : UserActivity
    {
        [Key]
        public Guid DetailTempRJId { get; set; }

        [Required]
        public Guid TempRJId { get; set; }

        [ForeignKey(nameof(TempRJId))]
        public RecurringJournal? RecurringJournal { get; set; }

        [Required]
        public Guid COAId { get; set; }

        [MaxLength(100)]
        public string? COACode { get; set; }

        [MaxLength(200)]
        public string? COAName { get; set; }

        /// <summary>
        /// Menjelaskan peran COA dalam recurring journal.
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

        public List<AccManualJurnalDetail>? AccManualJurnalDetails { get; set; }
    }
}