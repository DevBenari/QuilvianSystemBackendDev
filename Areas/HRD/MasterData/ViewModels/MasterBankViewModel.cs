using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.ViewModels
{
    public class MasterBankViewModel
    {
        [MaxLength(200)]
        public string? BankName { get; set; }

        [MaxLength(100)]
        public string? BankShortName { get; set; }
        public decimal? BiayaAdminBank { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }
    }
}
