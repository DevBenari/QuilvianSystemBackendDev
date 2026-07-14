using QuilvianSystemBackendDev.Areas.Finance.All.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.Finance.All.ViewModels
{
    public class RecurringJournalViewModel
    {
        public Guid TempRJId { get; set; }

        [Required]
        [MaxLength(200)]
        public string RecurringJournalName { get; set; } = string.Empty;

        public DateTime RecurringJournalDate { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }

    }
}
