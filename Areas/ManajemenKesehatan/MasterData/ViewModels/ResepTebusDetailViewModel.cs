namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class ResepTebusDetailViewModel
    {
        public Guid? ResepTebusId { get; set; }
        public Guid? RacikanId { get; set; }
        public bool? IsRacikan { get; set; }
        public Guid? ObatId { get; set; }
        public int? Qty { get; set; }
        public string? Signa { get; set; }
        public string? SignaTambahan { get; set; }
        public decimal? HargaObat { get; set; }
    }
}
