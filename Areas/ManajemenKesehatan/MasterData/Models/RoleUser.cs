using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("AspNetRoleUser", Schema = "dbo")]
    public class RoleUser
    {
        [Key]
        public Guid RoleUserId { get; set; }
        public Guid DepartemenId { get; set; }
        public Guid PositionId { get; set; }
    }
}
