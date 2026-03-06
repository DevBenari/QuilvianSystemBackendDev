namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels
{
    public sealed class PendapatanKasirHarianDto
    {
        public Guid? KasirUserId { get; set; }
        public string? PetugasKasir {  get; set; }
        public DateTime Tanggal { get; set; }
        public decimal PendapatanTunai { get; set; }
        public decimal PendapatanNonTunai { get; set; }
        public decimal PiutangAsuransi { get; set; }
        public decimal TotalPendapatan { get; set; }
    }
}
