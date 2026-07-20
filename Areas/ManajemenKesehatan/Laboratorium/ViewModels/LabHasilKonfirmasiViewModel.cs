namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels
{
    public class LabHasilKonfirmasiViewModel
    {
        public Guid? LabBookingId { get; set; }
        public Guid? DokterPerujukId { get; set; }
        public Guid? DokterKonfirmatorId { get; set; }
        public string? NoPhoneKonfirmator { get; set; }
        public bool? IsKonfirmatorDPJP { get; set; }
    }
}
