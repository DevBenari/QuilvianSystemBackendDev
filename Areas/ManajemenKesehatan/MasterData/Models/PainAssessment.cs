using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstPainAssessment", Schema = "public")]
    public class PainAssessment : UserActivity
    {
        [Key]
        public Guid PainAssessmentId { get; set; }
        public Guid? KunjunganId { get; set; }
        public string? KeluhanUtama { get; set; }

        [DefaultValue(false)]
        public bool? IsPain { get; set; }

        public string? Pemicu { get; set; }
        public string? Kualitas { get; set; }
        public string? Lokasi { get; set; }
        public Guid? SkalaPainId { get; set; }
        public string? Frekuensi { get; set; }
        public string? PainManagement { get; set; }

        [DefaultValue(false)]
        public bool? IsInheritedDisease { get; set; }

        public string? InheritedDisease { get; set; }

        [DefaultValue(false)]
        public bool? IsAlergic { get; set; }

        public string? Alergic { get; set; }
        public string? NafsuMakan { get; set; }

        [DefaultValue(false)]
        public bool? IsMual { get; set; }

        [DefaultValue(false)]
        public bool? IsMuntah { get; set; }

        [DefaultValue(false)]
        public bool? IsFallRisk { get; set; }

        public string? FallRisk { get; set; }
    }
}
