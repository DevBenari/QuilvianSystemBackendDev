namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.ViewModels
{
    public class OperasiTindakanViewModel
    {
        public Guid? TindakanId { get; set; }
        public Guid? JenisOperasiId { get; set; }
        public Guid? TipeOperasiId { get; set; }
        public string? Keterangan { get; set; }
    }
}
