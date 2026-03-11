namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels
{
    public class BillingManualVM
    {
        public Guid? KunjunganId { get; set; }
        public Guid? DiskonId { get; set; }
        public string? NamaItem { get; set; }
        public decimal? HargaItem { get; set; }
        public int? QtyItem { get; set; }
        public decimal? SubBiayaLainnya { get; set; }
    }
}
