using System.ComponentModel;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class PainAssessmentViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? UserActiveId { get; set; }
        public Guid? DelegasiId { get; set; }
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

        public bool? IsBCGimunisasi { get; set; }
        public bool? IsHepatitisBImunisasi { get; set; }
        public bool? IsPolioImunisasi { get; set; }
        public bool? IsDPTImunisasi { get; set; }
        public bool? IsCampakImunisasi { get; set; }
        public bool? IsAsiEksklusif { get; set; }
        public string? StatusMpasi { get; set; }
        public bool? IsAtaksia { get; set; }
        public bool? IsPosturalInstability { get; set; }
        public string? HasilResikoJatuh { get; set; }
        public bool? IsMotorikAktif { get; set; }
        public bool? IsResponsAuditori { get; set; }
        public bool? IsInteraksiSosial { get; set; }
        public Guid? RanapId { get; set; }
        public string? RPS { get; set; }
        public string? RPD { get; set; }
        public string? CurrentMedication { get; set; }
        public string? RiwayatPenyakit { get; set; }
        public bool? IsIGD { get; set; }
        public string? MasukIGD { get; set; }
        public string? KondisiMasukIGD { get; set; }
        public bool? IsPengobatanSaatIni { get; set; }
        public bool? IsTubuhTidakSeimbang { get; set; }
        public bool? IsMenggunakanPenopang { get; set; }
        public string? KeluhanTambahan { get; set; }
        public bool? IsFarmakologi { get; set; }
        public string? KeadaanUmum { get; set; }
        public string? IsKonjungtiva { get; set; }
        public string? Ekstremitas { get; set; }
        public string? SkorSedasi { get; set; }
    }
}
