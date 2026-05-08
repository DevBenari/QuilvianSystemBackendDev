namespace QuilvianSystemBackendDev.Areas.Finance.AR.ViewModels
{
    public class ARHeaderViewModel
    {
        public Guid AsuransiId { get; set; }

        public string NoInvoice { get; set; }

        public DateTime TglPembuatanInvoice { get; set; }

        // numeric biasanya dipetakan ke int atau decimal
        // kalau "DueDate" maksudnya hari jatuh tempo, pakai int
        public int DueDate { get; set; }

        public decimal TotalInvoice { get; set; }

        public DateTime? TglKirim { get; set; }
        public DateTime? TglTerima { get; set; }
        public DateTime? TglTagihan { get; set; }
        public DateTime? TglJatuhTempo { get; set; }

        public bool IsDocumentComplited { get; set; }

        public string Keterangan { get; set; }
    }
}
