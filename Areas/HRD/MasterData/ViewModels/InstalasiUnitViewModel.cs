using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.ViewModels
{
    public class InstalasiUnitViewModel
    {
        [Required]
        public string? NamaInstalasiUnit { get; set; }
        public Guid? DepartementId { get; set; }
        public string? Keterangan { get; set; }
    }
}
