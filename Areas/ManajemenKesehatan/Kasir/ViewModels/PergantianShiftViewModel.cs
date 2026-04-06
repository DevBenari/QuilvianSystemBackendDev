namespace QuilvianSystemBackendDev.ViewModels
{
    public class PergantianShiftViewModel
    {
        public string KodeRegistrasi { get; set; } = string.Empty;
        public Guid LayananId { get; set; }
        public Guid KasirId { get; set; }
        public Guid? LoketKasirId { get; set; }
        public string? StatusShift { get; set; }
        public string ShiftPergantian { get; set; } = string.Empty;
        public TimeSpan WaktuMulai { get; set; }
        public TimeSpan WaktuAkhir { get; set; }
        public DateTime TanggalPergantian { get; set; }
        public decimal SaldoAwal { get; set; }
        public decimal PendapatanTunai { get; set; }
        public decimal KasFisik { get; set; }
        public decimal SelisihPendapatanTunai { get; set; }
        public decimal TotalPendapatan { get; set; }
        public decimal PendapatanNonTunai { get; set; }
        public string? Keterangan { get; set; }
    }
}