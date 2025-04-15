namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class PainAssessmentViewModel
    {
        public Guid? KunjunganId { get; set; }
        public string? KeluhanUtama { get; set; }
        public bool? IsPain { get; set; }
        public string? Pemicu { get; set; }
        public string? Kualitas { get; set; }
        public string? Lokasi { get; set; }
        public Guid? SkalaPainId { get; set; }
        public string? Frekuensi { get; set; }
        public string? PainManagement { get; set; }
        public bool? IsInheritedDisease { get; set; }
        public string? InheritedDisease { get; set; }
        public bool? IsAlergic { get; set; }
        public string? Alergic { get; set; }
        public string? NafsuMakan { get; set; }
        public bool? IsMual { get; set; }
        public bool? IsMuntah { get; set; }
        public bool? IsFallRisk { get; set; }
        public string? FallRisk { get; set; }
    }
}
