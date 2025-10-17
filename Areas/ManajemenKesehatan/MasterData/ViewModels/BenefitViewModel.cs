namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class BenefitViewModel
    {
        public string? NamaBenefit { get; set; }
        public string? Keterangan { get; set; }
        public decimal? BiayaBenefit { get; set; }
        public bool? IsAktif { get; set; } = true;
    }
}
