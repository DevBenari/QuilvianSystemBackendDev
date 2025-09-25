using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    public class SlidingScale : UserActivity
    {
        [Key]
        public Guid SlidingScaleId { get; set; }   // Generate Otomatis (PK)
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public DateTime? TglSlidingScale { get; set; }
        public decimal? GDS { get; set; }
        public string? Insulin { get; set; }
        public string? InsulinDrip { get; set; }
        public Guid? UserActiveId { get; set; }
        public string? Keterangan { get; set; }
    }
}
