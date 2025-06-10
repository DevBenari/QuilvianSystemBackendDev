using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("BenefitEntitiyMapping", Schema = "public")]
    public class BenefitEntitiyMapping : UserActivity
    {
        [Key]
        public Guid BenefitEntitiyMappingId { get; set; }
        public Guid? BenefitId { get; set; }
        public Guid? EntityId { get; set; }
        public string? EntityType { get; set; } // Contoh: "Membership", "PendaftaranPasienBaru", dll.
        public decimal? Kuota{get; set;}
        public decimal? Diskon { get; set; }
        public bool? IsGratis { get; set; }
    }
}
