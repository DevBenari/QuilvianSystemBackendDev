using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models
{
    public class CttPemberianObat : UserActivity
    {
        [Key]
        public Guid CttPemberianObatId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? ObatId { get; set; }
        public Guid? RacikanId { get; set; }
        public DateTime? TglPemberian { get; set; }
        public TimeOnly? WaktuPemberian { get; set; }
        public string? StatusPemberian { get; set; }
        public string? CaraPemberianObat { get; set; }
        public Guid? UserActiveIdPerawat { get; set; }
        public Guid? TTDId { get; set; }
        public string? Keterangan { get; set; }
        public string? StatusCttEso { get; set; }

    }
}
