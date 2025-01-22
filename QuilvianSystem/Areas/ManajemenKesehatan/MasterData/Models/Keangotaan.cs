using QuilvianSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystem.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstKeangotaan", Schema = "dbo")]
    public class Keangotaan : UserActivity
    {
        [Key]
        public Guid KeangotaanId { get; set; }
        public string KeangotaanKode { get; set; }
        public string JenisKeangotaan { get; set; }
        public string JenisPromo { get; set; }
    }
}
