namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels
{
    public class StockBatchViewModel
    {
        public string? KodeBatch { get; set; }
        public Guid? ItemId { get; set; }
        public Guid? SupplierId { get; set; }
        public DateOnly? ExpiredDate { get; set; }
        public string? Keterangan { get; set; }
    }
}
