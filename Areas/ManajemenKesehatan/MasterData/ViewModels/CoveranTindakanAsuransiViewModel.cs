namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class CoveranTindakanAsuransiViewModel
    {
        public Guid? TindakanId { get; set; }
        public string? NamaTindakan { get; set; }
        public Guid? PoliklinikId { get; set; }
        public string? NamaPoliklinik { get; set; }
        public Guid? KelasId { get; set; }
        public string? NamaKelas { get; set; }
        public Guid? AsuransiId { get; set; }
        public decimal? TarifDokterAsuransi { get; set; }
        public decimal? TarifRsAsuransi { get; set; }
        public decimal? TarifJpAsuransi { get; set; }
        public decimal? TarifBahpAsuransi { get; set; }
        public decimal? TarifLainAsuransi { get; set; }
        public decimal? TarifTotalAsuransi { get; set; }
        public decimal? KSOAsuransi { get; set; }
    }
}
