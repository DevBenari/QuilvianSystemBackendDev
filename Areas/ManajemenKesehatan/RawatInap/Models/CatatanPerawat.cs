using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    public class CatatanPerawat : UserActivity
    {
        [Key]
        public Guid CatPerawatId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public Guid? PemeriksaanLabId { get; set; }
        public string? CatatanPerawatText {  get; set; }

    }
}
