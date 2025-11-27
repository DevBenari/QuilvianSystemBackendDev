using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Gizi.Models
{
    public class Recall : UserActivity
    {
        [Key]
        public Guid RecallId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public string? SikapPasienDiet { get; set; }
        public string? AnjuranDiet { get; set; }
        public DateTime? TglRecall { get; set; }
        public Guid? DietesienId { get; set; }
        public string? CatatanPerawat { get; set; }
    }
}
