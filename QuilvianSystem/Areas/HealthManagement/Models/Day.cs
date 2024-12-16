using QuilvianSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystem.Areas.HealthManagement.Models
{
    [Table("HmnDay", Schema = "dbo")]
    public class Day : UserActivity
    {
        [Key]
        public Guid DayId { get; set; }
        public string KodeHari { get; set; }
        public string NamaHari { get; set; }
    }
}
