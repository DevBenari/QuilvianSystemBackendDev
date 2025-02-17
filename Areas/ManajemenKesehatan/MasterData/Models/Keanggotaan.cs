using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstKeanggotaan", Schema = "dbo")]
    public class Keanggotaan : UserActivity
    {
        [Key]
        public Guid KeanggotaanId { get; set; }
        public string KodeKeanggotaan { get; set; }
        public string JenisKeanggotaan { get; set; }
        public string JenisPromo { get; set; }
    }
}
