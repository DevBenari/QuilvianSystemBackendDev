namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels
{
    public class StatusKonfirmasiBookingViewModel
    {
        public string? Status { get; set; }
        public DateTime? TglSampling { get; set; }
        public TimeOnly? WaktuKonfirmasi { get; set; }
        public Guid? DokterPemeriksaId { get; set; }
        public Guid? KonfirmatorId { get; set; }
    }
}
