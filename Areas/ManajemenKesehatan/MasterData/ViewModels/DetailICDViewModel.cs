namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class DetailICDViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? SoapId { get; set; }
        public Guid? ICDId { get; set; }
        public bool? isUtama { get; set; }
    }
}
