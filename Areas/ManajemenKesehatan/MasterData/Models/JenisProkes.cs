using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstJenisProkes", Schema = "public")]
    public class JenisProkes : UserActivity
    {
        [Key]
        public Guid JenisProkesId { get; set; }
        public string? NamaJenisProkes { get; set; }
        public string? Keterangan { get; set; }
    }
}
