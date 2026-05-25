namespace QuilvianSystemBackendDev.Areas.Finance.AyatSilangs.ViewModels
{
    public class AyatSilangViewModel
    {

        public string NoReferensi { get; set; }

        public string NoAyatSilang { get; set; }

        public Guid AsuransiId { get; set; }

        public Guid BankId { get; set; }

        public decimal TotalPembayaran { get; set; }

        public DateTime TglPembayaran { get; set; }

        public Guid UserProcess { get; set; }
        public bool? IsSudahTerpakai { get; set; }
        public string Keterangan { get; set; }
    }
}
