namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class SOAPViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? RanapId { get; set; }
        public string? Subjective { get; set; }
        public string? Objective { get; set; }
        public List<string>? DaftarICD10 { get; set; }
        public string? Assessment { get; set; }
        public string? Planning { get; set; }
        public string? Profesi { get; set; }
    }
}
