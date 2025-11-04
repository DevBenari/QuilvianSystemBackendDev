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
        public Guid? RanapId { get; set; }
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

        [DefaultValue(false)]
        public bool? IsBCGimunisasi { get; set; }

        [DefaultValue(false)]
        public bool? IsHepatitisBImunisasi { get; set; }

        [DefaultValue(false)]
        public bool? IsPolioImunisasi { get; set; }

        [DefaultValue(false)]
        public bool? IsDPTImunisasi { get; set; }

        [DefaultValue(false)]
        public bool? IsCampakImunisasi { get; set; }

        [DefaultValue(false)]
        public bool? IsAsiEksklusif { get; set; }
        public string? StatusMpasi { get; set; }

        [DefaultValue(false)]
        public bool? IsAtaksia { get; set; }

        [DefaultValue(false)]
        public bool? IsPosturalInstability { get; set; }

        public string? HasilResikoJatuh { get; set; }

        [DefaultValue(false)]
        public bool? IsMotorikAktif { get; set; }

        [DefaultValue(false)]
        public bool? IsResponsAuditori { get; set; }

        [DefaultValue(false)]
        public bool? IsInteraksiSosial { get; set; }
        public string? RPS { get; set; }
        public string? RPD { get; set; }
        public string? CurrentMedication { get; set; }
        public string? RiwayatPenyakit { get; set; }
        public bool? IsIGD { get; set; }
        public string? MasukIGD { get; set; }
        public string? KondisiMasukIGD { get; set ; }
        public bool? IsPengobatanSaatIni { get; set; }
        public bool? IsTubuhTidakSeimbang {  get; set; }
        public bool? IsMenggunakanPenopang {  get; set; }
        public string? KeluhanTambahan { get; set; }
        public bool? IsFarmakologi {  get; set; }
    }
}
