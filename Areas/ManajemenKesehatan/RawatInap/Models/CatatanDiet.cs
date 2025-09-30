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

        // Navigation property → otomatis jadi array/list
        public ICollection<CatatanDietDetail>? DetailIcd10 { get; set; } = new List<CatatanDietDetail>();
    }

    public class CatatanDietDetail : UserActivity
    {
        [Key]
        public Guid CatatanDietDetailId { get; set; }
        public Guid? CatatanDietId { get; set; }
        public Guid? Icd10Id { get; set; }

        public virtual CatatanDiet? CatatanDiet { get; set; }
    }
}
