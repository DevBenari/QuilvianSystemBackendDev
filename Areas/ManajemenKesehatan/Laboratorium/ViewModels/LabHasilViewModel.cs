namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels
{
    public class LabHasilViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? LabId { get; set; }
        public Guid? LabBookingId { get; set; }
        public List<Guid>? UserActiveId { get; set; } = new List<Guid>();
        public string? Keterangan { get; set; }
    }
}
