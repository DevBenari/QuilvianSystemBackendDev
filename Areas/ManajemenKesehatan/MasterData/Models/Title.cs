using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstTitle", Schema = "dbo")]
    public class Title : UserActivity
    {
        [Key]
        public Guid TitleId { get; set; }
        public string KodeTitle { get; set; }
        public string NamaTitle { get; set; }
    }
}
