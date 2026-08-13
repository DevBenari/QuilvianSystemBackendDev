namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels
{
    public class LabHasilBakteriViewModel
    {
        public Guid? LabHasilId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public Guid? LabBookingId { get; set; }
        public Guid? MappingBakteriId { get; set; }
        public string? Keterangan { get; set; }
    }
}
