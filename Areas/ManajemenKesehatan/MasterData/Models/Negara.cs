using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models

{
    [Table("MstNegara", Schema = "dbo")]
    public class Negara : UserActivity
    {
        [Key]
        public Guid NegaraId { get; set; }
        public string KodeNegara { get; set; }
        public string NamaNegara { get; set; }
    }
}
