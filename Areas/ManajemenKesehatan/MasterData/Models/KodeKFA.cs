using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstKodeKFA", Schema = "public")]
    public class KodeKFA : UserActivity
    {
        [Key]
        public Guid KFAId { get; set; }
        public string? NamaKode { get; set; }
        public string? NamaKFA { get; set; }
        public string? Keterangan { get; set; }
    }
}
