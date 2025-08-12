using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class DetailPlanningViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? SoapId { get; set; }
        public string? Keterangan { get; set; }
    }
}
