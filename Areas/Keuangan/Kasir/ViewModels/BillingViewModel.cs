namespace QuilvianSystemBackendDev.Areas.Keuangan.Kasir.ViewModels
{
    public class BillingViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? DiskonId { get; set; }
        public DateTime? BillingDate { get; set; }
        public string? BillingKode { get; set; }
        public Guid? ItemId { get; set; }
        public string? NamaItem { get; set; }
        public decimal? HargaItem { get; set; }
        public decimal? SubTotalItem { get; set; }
        public decimal? Keterangan { get; set; }
    }
}
