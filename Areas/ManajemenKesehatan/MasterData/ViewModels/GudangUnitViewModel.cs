namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class GudangUnitViewModel
    {
        public Guid? GudangId { get; set; }
        public Guid? ObatId { get; set; }
        public decimal? StockGudangUnit { get; set; }
        public decimal? MinStockGudangUnit { get; set; }
        public decimal? MaxStockGudangUnit { get; set; }
        public decimal? StockPenyanggaGudangUnit { get; set; }
        public string? Keterangan { get; set; }
    }
}
