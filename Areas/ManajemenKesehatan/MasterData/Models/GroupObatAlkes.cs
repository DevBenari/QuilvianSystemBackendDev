using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstGroupObatAlkes", Schema = "public")]
    public class GroupObatAlkes : UserActivity
    {
        [Key]
        public Guid GroupObatAlkesId { get; set; }
        public string? NamaGroupObatAlkes {  get; set; }
        public string? Keterangan {  get; set; }
    }
}
