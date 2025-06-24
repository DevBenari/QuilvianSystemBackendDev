namespace QuilvianSystemBackendDev.Areas.Keuangan.Kasir.ViewModels
{
    public class KasirTebusResepViewModel
    {
        public Guid? ResepTebusId { get; set; }
        public Guid? PaymentMethodId { get; set; }
        public string? NamaMetode { get; set; }
        public bool? StatusPembayaran { get; set; }
        public string? Keterangan { get; set; }
    }
}
