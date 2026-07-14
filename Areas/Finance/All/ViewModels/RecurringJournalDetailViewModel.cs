using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.Finance.All.ViewModels
{
    public class RecurringJournalDetailViewModel
    {
        [Required]
        public Guid TempRJId { get; set; }

        [Required]
        public Guid COAId { get; set; }

        [MaxLength(100)]
        public string? COACode { get; set; }

        [MaxLength(200)]
        public string? COAName { get; set; }

        [MaxLength(200)]
        public string? RoleSetupCOA { get; set; }

        public decimal DebetAmount { get; set; }

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