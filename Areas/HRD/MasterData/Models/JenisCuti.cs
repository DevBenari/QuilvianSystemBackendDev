using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.Models
{
    [Table("Hrd_MstJenisCuti", Schema = "public")]
    public class JenisCuti : UserActivity
    {
        [Key]
        public Guid JenisCutiId { get; set; }
        public string? NamaCuti { get; set; }
        public string? KuotaTahunan { get; set; }
        public string? Keterangan { get; set; }
    }
}
