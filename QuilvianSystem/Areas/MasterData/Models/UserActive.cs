using QuilvianSystem.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystem.Areas.MasterData.Models
{
    [Table("MasterUserActive", Schema = "dbo")]
    public class UserActive : UserActivity
    {
        public Guid UserActiveId { get; set; }
        public string UserActiveCode { get; set; }
        public string FullName { get; set; }
        public string IdentityNumber { get; set; }
        public string PlaceOfBirth { get; set; }
        public DateTimeOffset DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string Handphone { get; set; }
        public string Email { get; set; }
        public string? Foto { get; set; }
        public bool IsActive { get; set; }

    }
}
