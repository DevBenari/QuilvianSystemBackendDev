namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class MapAntibiotikSubBakteriViewModel
    {
        public Guid? SubBakteriId { get; set; }
        public Guid? AntibiotikId { get; set; }
        public decimal? NormalMin { get; set; }
        public decimal? NormalMax { get; set; }
        public decimal? CriticalMin { get; set; }
        public decimal? CriticalMax { get; set; }
        public decimal? UrutanTampil { get; set; }
        public string? Keterangan { get; set; }
    }
}
