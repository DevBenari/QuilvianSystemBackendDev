using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstJenisProdukObat", Schema = "public")]
    public class JenisProdukObat : UserActivity
    {
        [Key]
        public Guid JenisProdukObatId { get; set; }
        public string? NamaJenisProdukObat { get; set; }
        public string? Keterangan { get; set; }
    }
}
