namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class DetailDiskonViewModel
    {
        public Guid? DiskonId { get; set; }
        public Guid? LayananId { get; set; }
        public Guid? ItemId { get; set; }
        public string? KodeLayanan { get; set; }
        public string? KategoriLayanan { get; set; }
        public decimal? MaxQty { get; set; }
        public decimal? MaxHarga { get; set; }
        public string? Keterangan { get; set; }
    }
}
