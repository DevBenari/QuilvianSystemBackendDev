namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels
{
    public class CatatanPerawatViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public Guid? TindakanId { get; set; }
        public string? CatatanPerawatText { get; set; }
    }
}
