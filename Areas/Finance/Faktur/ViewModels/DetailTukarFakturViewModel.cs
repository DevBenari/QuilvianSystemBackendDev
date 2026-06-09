namespace QuilvianSystemBackendDev.Areas.Finance.Faktur.ViewModels
{
    public class DetailTukarFakturViewModel
    {
        public Guid? DetailTukarFakturId { get; set; }
        public Guid TukarFakturId { get; set; }
        public string NomorPO { get; set; }
        public string NoInvoice { get; set; }

        public decimal TotalInvoice { get; set; }

        public string Keterangan { get; set; }
    }
}
