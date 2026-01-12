namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels
{
    public class HandoverPasienViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public DateTime? TanggalSerahTerima { get; set; }
        public Guid? AdministrationId { get; set; }
        public Guid? CROId { get; set; }
        public Guid? PerawatId { get; set; }
        public string? Keterangan { get; set; }
        public List<HandoverPasienDetailViewModel>? Details { get; set; }
    }
}
