namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels
{
    public class SuratPengantarRawatInapViewModel
    {
        public Guid? KunjunganId { get; set; }
        public string? Diagnosa { get; set; }
        public Guid? ICDId { get; set; }
        public string? AlasanRanap { get; set; }
        public string? RencanaTindakLanjut { get; set; }
        public string? AsalUnit { get; set; }
    }
}
