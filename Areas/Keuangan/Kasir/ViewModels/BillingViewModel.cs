namespace QuilvianSystemBackendDev.Areas.Keuangan.Kasir.ViewModels
{
    public class BillingViewModel
    {
        public Guid? DiskonId { get; set; }
        public int? QtyItem { get; set; }
        public decimal? SubTotalItem { get; set; }
        public decimal? Keterangan { get; set; }
    }
}
