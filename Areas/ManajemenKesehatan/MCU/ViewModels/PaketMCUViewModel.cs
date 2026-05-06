namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MCU.ViewModels
{
    public class PaketMCUViewModel
    {
        public Guid? PemeriksaanLabId { get; set; }
        public Guid? ModulMCUId { get; set; }
        public Guid? DokterID { get; set; }
        public string? Keterangan { get; set; }
    }
}
