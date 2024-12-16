using QuilvianSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystem.Areas.HealthManagement.Models
{
    [Table("HmnDoctorQueueType", Schema = "dbo")]
    public class DoctorQueueType : UserActivity //UserActivity Create on Repository
    {
        [Key]
        public Guid DoctorQueueTypeId { get; set; }
        public string KodeTipeAntrian{ get; set; }
        public string NamaTipeAntrian { get; set; }
    }
}
