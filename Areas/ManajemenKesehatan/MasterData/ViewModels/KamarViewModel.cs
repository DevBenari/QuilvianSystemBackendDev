namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class KamarViewModel
    {
        public Guid? KelasId { get; set; }
        public string? NamaKamar { get; set; }
        public decimal? TarifHarian { get; set; }
        public string? Lantai { get; set; }
        public string? PosisiRuangan { get; set; }
        public string? Deskripsi { get; set; }
    }
}
