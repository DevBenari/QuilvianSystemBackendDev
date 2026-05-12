namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class ObatUnitViewModel
    {
        public Guid? ObatId { get; set; }
        public Guid? InstalasiUnitId { get; set; }
        public decimal? Qty { get; set; }
        public decimal? QtyAmbil { get; set; }
        public decimal? QtyTersedia { get; set; }
    }
}
