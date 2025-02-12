using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstIdentitas", Schema = "dbo")]
    public class Identitas : UserActivity

    {
        public Guid IdentitasId { get; set; }
        public string KdIdentitas { get; set; }
        public string JenisIdentitas { get; set; }

    }
}
