using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    public class DetailPlanning : UserActivity
    {
        [Key]
        public Guid DetailPlanningId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? SoapId { get; set; }

        [Column(TypeName = "text")]
        public string? Keterangan { get; set; }
    }
}
