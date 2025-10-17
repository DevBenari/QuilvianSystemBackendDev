namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels
{
    public class ResepDetailViewModel
    {
        public Guid? ResepId { get; set; }
        public Guid? AsuransiId { get; set; }
        public string? NamaAsuransi { get; set; }
        public Guid? ObatId { get; set; }
        public int? Qty { get; set; }
        public decimal? TakaranDosis { get; set; }
        public string? Signa { get; set; }
        public string? SignaTambahan { get; set; }
        public string? JenisObat { get; set; }
        public decimal? HargaObat { get; set; }
        public bool? StatusCoverObat { get; set; } = false;
        public bool? IsRacikan { get; set; } // "Ya" or "Tidak"
        public bool? IsContinued { get; set; }
        public bool? StatusDiberikanPasien { get; set; }

        public List<RacikanViewModel>? Racikan { get; set; }
        public string? KeteranganRacikan { get; set; }
        public string? EstimasiPemberian { get; set; }
        public string? CaraPemakaian { get; set; }
        public string? TglStopPemakaian { get; set; }

        public bool? ObatPagiDiambil { get; set; }
        public bool? ObatSiangDiambil { get; set; }
        public bool? ObatMalamDiambil { get; set; }
        //public decimal? JumlahIteratur { get; set; }
        //public string? TglMulaiIteratur { get; set; }
        //public decimal? JarakPenebusan { get; set; }
        //public string? MasaAktifIteratur { get; set; }
        //public bool? IsIteratur { get; set; } = false;
    }
}
