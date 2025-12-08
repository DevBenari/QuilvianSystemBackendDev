using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Administrator.MasterData.Models
{
    [Table("Fingerprint", Schema = "public")]
    public class Fingerprint : UserActivity
    {
        [Key]
        public Guid FingerprintId { get; set; }
        public string UserId { get; set; }
        public string DeviceId { get; set; }
        [Column(TypeName = "text")]
        public string Template { get; set; } // template fingerprint base64
        public string Status { get; set; }
    }
}
