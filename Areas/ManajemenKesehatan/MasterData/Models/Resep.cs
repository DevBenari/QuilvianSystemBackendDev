using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstResep", Schema = "public")]
    public class Resep : UserActivity
    {
        [Key]
        public Guid ResepId { get; set; }
        public Guid? KunjunganId { get; set; }
    }
}
