using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstBentukObat", Schema = "public")]
    public class BentukObat : UserActivity
    {
        [Key]
        public Guid BentukSatuanId { get; set; }
        public string KodeBentukSatuan { get; set; }
        public string NamaBentukSatuan { get; set; }
    }
}
