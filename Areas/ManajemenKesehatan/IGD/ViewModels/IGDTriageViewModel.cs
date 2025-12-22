namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.ViewModels
{
    public class IGDTriageViewModel
    {
        public Guid? KunjunganId { get; set; }
        public string? KeluhanUtama { get; set; }
        public string? DiteruskanKepada { get; set; }
        public string? DikirimKe { get; set; }
        public string? Keterangan { get; set; }
        public bool? Status { get; set; }
        public List<IGDTriageDetailViewModel>? Details { get; set; }

    }
}
