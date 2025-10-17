using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.Models
{
    [Table("Hrd_MstJenisTiketing", Schema = "public")]
    public class JenisTiketing : UserActivity
    {
        [Key]
        public Guid JenisTicketId { get; set; }

        public Guid DepartementId { get; set; }

        public string NamaTicket { get; set; } = string.Empty;

        public string? Keterangan { get; set; }
    }
}
