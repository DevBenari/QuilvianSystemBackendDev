namespace QuilvianSystemBackendDev.Areas.Finance.Po.ViewModels
{
    public class PurchaseOrderItemViewModel
    {
        public string? ProductName { get; set; }
        public string? Measurement { get; set; }
        public string? Category { get; set; }
        public string? Layanan { get; set; }
        public string? JenisPermintaan { get; set; }

        public decimal? Qty { get; set; }
        public decimal? Price { get; set; }
        public decimal? Discount { get; set; }
        public decimal? SubTotal { get; set; }

        public string? Keterangan { get; set; }
    }
}
