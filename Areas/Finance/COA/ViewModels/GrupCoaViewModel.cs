using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.Finance.COA.ViewModels
{
    public class GrupCoaViewModel
    {
        public Guid GrupCOAId { get; set; }

        //public Guid? TipeAkunCOAId { get; set; }

        [MaxLength(200)]
        public string? NamaGrupCOA { get; set; }

        [MaxLength(50)]
        public string? KodeGrupCOA { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }
    }
}
