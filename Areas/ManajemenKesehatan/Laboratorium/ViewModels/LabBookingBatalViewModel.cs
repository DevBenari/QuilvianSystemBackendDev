namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels
{
    public class LabBookingBatalViewModel
    {
        public Guid? LabBookingId { get; set; }
        public Guid? DetailLabBookingId { get; set; }
        public string? JenisPembatalan { get; set; } = string.Empty;
        public DateTime? TglPembatalan { get; set; }
        public string? Keterangan { get; set; }
    }
}
