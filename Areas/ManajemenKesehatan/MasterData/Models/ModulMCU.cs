using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstModulMCU", Schema = "public")]
    public class ModulMCU : UserActivity
    {
        [Key]
        public Guid ModulMCUId { get; set; }
        public string? NamaModul {  get; set; }
        public string? Keterangan {  get; set; }
    }
}
