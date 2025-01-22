using QuilvianSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystem.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstJabatan", Schema = "dbo")]
    public class Jabatan : UserActivity
    {
        [Key]
        public Guid JabatanId { get; set; }
        public string JabatanKode { get; set; }
        public string JenisJabatan { get; set; }
    }
}
