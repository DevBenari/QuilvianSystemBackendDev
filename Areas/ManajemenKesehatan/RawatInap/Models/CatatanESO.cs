using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    public class CatatanESO : UserActivity
    {
        [Key]
        public Guid? ESOId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? CttPemberianObatId { get; set; }
        public Guid? ObatId { get; set; }
        public Guid? RacikanId { get; set; }
        public bool? IsTandaiObat { get; set; }
        public DateTime? TglTerjadi { get; set; }
        public string? ManifestasiESO { get; set; }
        public DateTime? TglKesudahan { get; set; }
        public Guid? PerawatUserActiveId { get; set; }
        public string? TTDPath { get; set; }
        public string? Keterangan {  get; set; }
    }
}
