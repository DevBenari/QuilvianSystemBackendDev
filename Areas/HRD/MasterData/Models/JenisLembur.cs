using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.Models
{
    [Table("Hrd_MstJenisLembur", Schema = "public")]
    public class JenisLembur : UserActivity
    {
        [Key]
        public Guid JenisLemburId { get; set; }
        public string? NamaLembur { get; set; }
        public string? Keterangan { get; set; }
    }
}
