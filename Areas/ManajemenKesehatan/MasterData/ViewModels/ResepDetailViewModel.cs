namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class ResepDetailViewModel
    {
        public Guid? ResepId { get; set; }
        public Guid? AsuransiId { get; set; }
        public string? NamaAsuransi { get; set; }
        public Guid? ObatId { get; set; }
        public int? Qty { get; set; }
        public string? Signa { get; set; }
        public string? SignaTambahan { get; set; }
        public string? InteraturObat { get; set; }
        public string? JenisObat { get; set; }
        public decimal? HargaObat { get; set; }
        public bool? StatusCoverObat { get; set; } = false;
        public Guid? RacikanId { get; set; }
        public string? IsRacikan { get; set; } // "Ya" or "Tidak"
    }
}
