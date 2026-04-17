using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstLantai", Schema = "public")]
    public class Lantai : UserActivity
    {
        [Key]
        public Guid LantaiId { get; set; }
        public string? KodeBarang { get; set; }
        public string? NamaLantai { get; set; }
        public string? Keterangan {  get; set; }
    }
}
