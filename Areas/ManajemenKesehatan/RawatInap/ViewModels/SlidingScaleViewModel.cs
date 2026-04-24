namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels
{
    public class SlidingScaleViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public string? TglSlidingScale { get; set; }
        public decimal? GDS { get; set; }
        public string? Insulin { get; set; }
        public string? InsulinDrip { get; set; }
        public Guid? UserActiveId { get; set; }
        public string? Keterangan { get; set; }
    }
}
