using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    public class SOAPPlanning : UserActivity
    {
        [Key]
        public Guid SOAPPlanningId { get; set; }
        public Guid? IcdId { get; set; }
        public Guid? PlanningIcdId { get; set; }
        [Column(TypeName = "text")]
        public string? Keterangan { get; set; }
    }
}
