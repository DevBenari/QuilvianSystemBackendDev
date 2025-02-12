using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstAgama", Schema = "dbo")]
    public class Agama : UserActivity
    {
        [Key]
        public Guid AgamaId { get; set; }
        public string KodeAgama { get; set; }
        public string NamaAgama { get; set; }
    }
}
