namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels
{
    public class IndikatorPengkajianViewModel
    {
        public Guid? IndikatorId { get; set; }
        public Guid? IndikatorScoreId { get; set; }
        public Guid? KategoriIndikatorId { get; set; }
        public string? Keterangan { get; set; }
    }
}
