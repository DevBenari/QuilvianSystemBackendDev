using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstJabatan", Schema = "public")]
    public class Jabatan : UserActivity
    {
        [Key]
        public Guid JabatanId { get; set; }
        public string KodeJabatan { get; set; }
        public string NamaJabatan { get; set; }
    }
}
