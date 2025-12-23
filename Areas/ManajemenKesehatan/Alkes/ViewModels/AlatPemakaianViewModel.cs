namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Alkes.ViewModels
{
    public class AlatPemakaianViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public DateTime? TanggalPemakaian { get; set; }
        public string? Keterangan { get; set; }
        public List<AlatPemakaianDetailViewModel>? Details {  get; set; }
    }
}
