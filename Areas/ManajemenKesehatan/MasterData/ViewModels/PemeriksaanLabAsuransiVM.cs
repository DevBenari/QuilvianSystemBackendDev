namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class PemeriksaanLabAsuransiVM
    {
        public Guid? PemeriksaanLabId { get; set; }
        public Guid? AsuransiId { get; set; }
        public decimal? Diskon { get; set; }
    }
}
