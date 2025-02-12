using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("AspNetUserRole", Schema = "dbo")]
    public class RoleUser
    {
        [Key]
        public Guid Id { get; set; }
        public string RoleId { get; set; }
        public string PositionId { get; set; }
    }
}
