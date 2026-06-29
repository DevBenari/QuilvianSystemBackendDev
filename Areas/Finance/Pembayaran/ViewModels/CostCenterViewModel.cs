using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.Finance.Pembayaran.ViewModels
{
    public class CostCenterViewModel
    {
        [Required]
        [MaxLength(250)]
        public string? LokasiCostCenter { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }
    }
}