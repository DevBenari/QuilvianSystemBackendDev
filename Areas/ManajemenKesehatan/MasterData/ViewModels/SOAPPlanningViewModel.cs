using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class SOAPPlanningViewModel
    {
        public Guid? IcdId { get; set; }
        public Guid? PlanningIcdId { get; set; }
        public string? Keterangan { get; set; }
    }
}
