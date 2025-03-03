using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstKategoriPeralatan", Schema = "public")]
    public class KategoriPeralatan : UserActivity
    {
        [Key]
        public Guid KategoriPeralatanId { get; set; }
        public string KodeKategoriPeralatan { get; set; }
        public string NamaKategoriPeralatan { get; set; }
    }
}
