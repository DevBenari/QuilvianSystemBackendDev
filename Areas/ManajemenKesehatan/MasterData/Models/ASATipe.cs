using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstASATipe", Schema = "public")]
    public class ASATipe : UserActivity
    {
        [Key]
        public Guid TipeASAId { get; set; }
        public string? NamaTipeASA { get; set; }
        public string? Keterangan {  get; set; }
    }
}
