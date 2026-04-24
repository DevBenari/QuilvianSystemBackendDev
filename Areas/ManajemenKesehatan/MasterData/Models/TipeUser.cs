using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstTipeUser", Schema = "public")]
    public class TipeUser : UserActivity
    {
        [Key]
        public Guid TipeUserId { get; set; }
        public string KodeTipeUser { get; set; }
        public string NamaTipeUser { get; set; }
    }
}
