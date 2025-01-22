using QuilvianSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystem.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstAgama", Schema = "dbo")]
    public class Agama : UserActivity
    {
        [Key]
        public Guid AgamaId { get; set; }
        public string AgamaKode { get; set; }
        public string JenisAgama { get; set; }
    }
}
