using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("PasienBenefitAsign", Schema = "public")]
    public class PasienBenefitAsign : UserActivity
    {
        [Key]
        public Guid BenefitAsignId { get; set; }
        public Guid? PasienId { get; set; }
        public Guid? BenefitId { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
