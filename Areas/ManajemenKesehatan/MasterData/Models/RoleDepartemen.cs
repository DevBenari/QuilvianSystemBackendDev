using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("AspNetRoleDepartemen", Schema = "dbo")]
    public class RoleDepartemen : UserActivity
    {
        [Key]
        public Guid RoleDepartemenId { get; set; }
        public Guid RolePositionId { get; set; }
        public Guid DepartemenId { get; set; }
    }

}

