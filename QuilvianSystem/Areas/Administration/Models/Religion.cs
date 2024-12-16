using QuilvianSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystem.Areas.Administration.Models
{
    [Table("AdmReligion", Schema = "dbo")]
    public class Religion : UserActivity //UserActivity Create on Repository
    {
        [Key]
        public Guid ReligionId { get; set; }
        public string KodeAgama { get; set; }
        public string NamaAgama { get; set; }
    }
}
