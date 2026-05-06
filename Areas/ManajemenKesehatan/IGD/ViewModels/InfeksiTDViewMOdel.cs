namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.ViewModels
{
    public class InfeksiTDViewMOdel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public DateTime? TglTransfusi { get; set; }
        public string? JenisTransfusi { get; set; }
        public decimal? Jumlah { get; set; }
        public DateTime? TglPencatatan { get; set; }
        public string? Keterangan { get; set; }
        public List<InfeksiDetailViewModel> Details { get; set; }
    }
}
