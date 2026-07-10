using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Finance.All.Models
{
    [Table("Fin_TempRecurringJournal", Schema = "public")]
    public class RecurringJournal : UserActivity
    {
        [Key]
        public Guid TempRJId { get; set; }

        [Required]
        [MaxLength(200)]
        public string RecurringJournalName { get; set; } = string.Empty;

        public DateTime RecurringJournalDate { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }

        public List<RecurringJournalDetail>? RecurringJournalDetails { get; set; }

        public List<AccManualJurnal>? AccManualJurnals { get; set; }
    }
}