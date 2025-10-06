using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    public class CatatanDiet : UserActivity
    {
        [Key]
        public Guid CatatanDietId { get; set; }
        public Guid KunjunganId { get; set; }
        public Guid PasienId { get; set; }
        public string? Diet { get; set; }
        public string? StatusDiet { get; set; }
        public string? Keterangan { get; set; }
        public DateTime? TglCatatanDiet { get; set; }
        public string? Diagnosa {  get; set; }
    }

}
