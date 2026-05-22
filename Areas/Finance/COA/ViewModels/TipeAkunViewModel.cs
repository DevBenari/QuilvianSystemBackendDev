using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.Finance.COA.ViewModels
{
    public class TipeAkunViewModel
    {

        [MaxLength(200)]
        public string? NamaTipeAkunCOA { get; set; }

        [MaxLength(50)]
        public string? KodeTipeAkunCOA { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }
    }
}
