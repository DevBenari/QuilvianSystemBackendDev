namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class TindakanKunjunganViewModel
    {
        public Guid KunjunganId { get; set; }
        public Guid TindakanId { get; set; }
        public int? Quantity { get; set; }
        //public decimal? Total { get; set; }
        public string? Disposition { get; set; }
        public Guid? DiskonId { get; set; }
    }
}
