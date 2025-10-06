namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class SOAPViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? RanapId { get; set; }
        public string? Subjective { get; set; }
        public string? Objective { get; set; }
        public List<Guid>? DaftarICD10 { get; set; }
        public List<Guid>? DaftarSDKI { get; set; }
        public string? Assessment { get; set; }
        public string? Planning { get; set; }
        public string? Profesi { get; set; }
        public string? Evaluasi { get; set; }
        public string? Intervensi { get; set; }
        public string? Reevaluasi { get; set; }
    }
}
