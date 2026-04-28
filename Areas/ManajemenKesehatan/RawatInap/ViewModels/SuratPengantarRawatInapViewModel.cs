namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels
{
    public class SuratPengantarRawatInapViewModel
    {
        public Guid? KunjunganId { get; set; }
        public string? Diagnosa { get; set; }
        public Guid? ICDId { get; set; }
        //public decimal? DepositRanap { get; set; }
        public string? AlasanRanap { get; set; }
        public string? RencanaTindakLanjut { get; set; }
        public string? AsalUnit { get; set; }
        public string? IndikasiTindakan { get; set; }
        public string? JenisOperasi { get; set; }
        public string? TawaranLayanan { get; set; }
        public string? HarapanHasil { get; set; }
        public bool? IsAdaHambatan { get; set; }
        public Guid? UserActiveDokterId { get; set; }
    }
}
