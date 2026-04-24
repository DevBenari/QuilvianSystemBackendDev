using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstOperasiJenis", Schema = "public")]
    public class OperasiJenis : UserActivity
    {
        [Key]
        public Guid JenisOperasiId { get; set; }
        public string? NamaJenisOperasi { get; set; }
        public string? Keterangan {  get; set; }
    }
}
