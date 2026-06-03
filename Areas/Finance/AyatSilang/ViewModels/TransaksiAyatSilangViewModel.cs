namespace QuilvianSystemBackendDev.Areas.Finance.AyatSilangs.ViewModels
{
    public class TransaksiAyatSilangViewModel
    {

        public Guid AyatSilangId { get; set; }

        public DateTime TglTransaksiMasuk { get; set; }

        public decimal SaldoKredit { get; set; }

        public DateTime TglTransaksiKeluar { get; set; }

        public decimal SaldoDebet { get; set; }

        public string Keterangan { get; set; }
    }
}
