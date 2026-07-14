using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.Finance.All.ViewModels
{
    public class AccManualJurnalViewModel
    {
        [Required]
        [MaxLength(100)]
        public string KodeManualJurnal { get; set; } = string.Empty;

        [Required]
        public DateTime TglDokumen { get; set; }

        [Required]
        public DateTime TglManualJurnal { get; set; }

        public DateTime? TglPembatalan { get; set; }

        [Required]
        [MaxLength(100)]
        public string TipeDokumen { get; set; } = string.Empty;

        public Guid? TempRJId { get; set; }

        [MaxLength(200)]
        public string? RecurringJournalName { get; set; }

        public DateTime? RecurringJournalDate { get; set; }

        [Required]
        public Guid MataUangId { get; set; }

        [MaxLength(100)]
        public string? NamaMataUang { get; set; }

        [Required]
        public Guid ExchangeRateId { get; set; }

        public decimal RateToIdr { get; set; }

        public decimal UnbalancedAmount { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }
    }
}