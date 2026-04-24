using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstAnastesiTipe", Schema = "public")]
    public class AnastesiTipe : UserActivity
    {
        [Key]
        public Guid TipeAnastesiId { get; set; }
        public string? NamaTipeAnastesi {  get; set; }
        public string? Keterangan {  get; set; }
    }
}
