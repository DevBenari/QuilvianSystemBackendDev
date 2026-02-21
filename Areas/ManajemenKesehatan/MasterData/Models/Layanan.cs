using QuilvianSystemBackendDev.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstLayanan", Schema = "public")]
    public class Layanan : UserActivity
    {
        [Key]
        public Guid LayananId { get; set; }
        public string? NamaLayanan { get; set; }
    }
}
