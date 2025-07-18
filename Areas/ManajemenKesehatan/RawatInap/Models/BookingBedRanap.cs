using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    [Table("BookingBedRanap", Schema = "public")]
    public class BookingBedRanap : UserActivity
    {
        [Key]
        public Guid BookingBedRanapId { get; set; }
        public Guid? RanapId { get; set; }
        public Guid? KamarId { get; set; }
        public Guid? BedId { get; set; }
        public DateTime? TglMasuk { get; set; }
        public DateTime? TglKeluar { get; set; }
        public bool? StatusBed { get; set; }
        public string? Keterangan { get; set; }
    }
}
