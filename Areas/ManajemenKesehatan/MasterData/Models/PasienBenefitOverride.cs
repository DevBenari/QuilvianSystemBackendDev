using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("PasienBenefitOverride", Schema = "public")]
    public class PasienBenefitOverride : UserActivity
    {
        [Key]
        public Guid BenefitOverrideId { get; set; }
        public Guid? PasienId { get; set; }
        public Guid? EntityId { get; set; }
        public string? EntityType { get; set; }
        public string? Sumber { get; set; } // Contoh: "Membership", "PendaftaranPasienBaru", dll.
        public decimal? BiayaTambahan { get; set; }
        public bool? Diskon { get; set; }
        public bool? IsAktif { get; set; } = true;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
