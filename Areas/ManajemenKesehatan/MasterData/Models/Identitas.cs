using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstIdentitas", Schema = "public")]
    public class Identitas : UserActivity
    {
        public Guid IdentitasId { get; set; }
        public string KodeIdentitas { get; set; }
        public string JenisIdentitas { get; set; }
    }
}
