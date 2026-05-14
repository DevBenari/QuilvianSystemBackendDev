namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels
{
    public class ResepTebusDetailViewModel
    {
        public Guid? ResepTebusId { get; set; }
        public Guid? RacikanId { get; set; }
        public Guid? ObatUnitId { get; set; }
        public Guid? InstalasiUnitId { get; set; }
        public bool? IsRacikan { get; set; }
        public Guid? ObatId { get; set; }
        public int? Qty { get; set; }
        public string? Signa { get; set; }
        public string? SignaTambahan { get; set; }
        public decimal? HargaObat { get; set; }
    }
}
