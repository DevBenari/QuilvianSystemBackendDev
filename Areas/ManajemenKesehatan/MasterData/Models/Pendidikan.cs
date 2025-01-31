using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstPendidikan", Schema = "dbo")]
    public class Pendidikan : UserActivity
    {
        [Key]
        public Guid PendidikanId { get; set; }
        public string KodePendidikan { get; set; }
        public string NamaPendidikan { get; set; }
    }
}
