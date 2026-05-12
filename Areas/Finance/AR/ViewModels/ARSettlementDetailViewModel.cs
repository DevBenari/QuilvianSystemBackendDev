namespace QuilvianSystemBackendDev.Areas.Finance.AR.ViewModels
{
    public class ARSettlementDetailViewModel
    {
        public Guid SettlementARId { get; set; }

        public string NoRegistrasi { get; set; } = string.Empty;

        public string NoBill { get; set; } = string.Empty;

        public string NoInvoice { get; set; } = string.Empty;

        public DateTime TglTransaksi { get; set; }

        public decimal JumlahUang { get; set; }

        public decimal Saldo { get; set; }

        public int PembayaranKe { get; set; }

        public bool IsCanceled { get; set; }

        public string User { get; set; } = string.Empty;

        public string TipeSettlement { get; set; } = string.Empty;

        public string Keterangan { get; set; } = string.Empty;
    }
}
