namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.ViewModels
{
    public class IGDTindakanDetailViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? TindakanId { get; set; }
        public string? Keterangan { get; set; }
        public string? KategoriTindakan { get; set; }
        public DateTime? WaktuTindakan { get; set; }
        public IFormFile? TTDFile { get; set; }
    }
}
