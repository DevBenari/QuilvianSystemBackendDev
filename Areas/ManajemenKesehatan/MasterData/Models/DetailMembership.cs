using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("DetailMembership", Schema = "public")]
    public class DetailMembership : UserActivity
    {
        [Key]
        public Guid DetailMembershipId { get; set; }
        public Guid? MembershipId { get; set; }
        public Guid? BenefitId { get; set; }
        public string? Keterangan { get; set; }
        public bool? IsActive { get; set; } = true;
    }
}
