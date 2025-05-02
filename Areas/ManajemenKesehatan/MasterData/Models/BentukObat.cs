using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstBentukObat", Schema = "public")]
    public class BentukObat
    {
        [Key]
        public Guid BentukObatId { get; set; }
        public string KodeBentukObat { get; set; }
        public string NamaBentukObat { get; set; }
    }
}
