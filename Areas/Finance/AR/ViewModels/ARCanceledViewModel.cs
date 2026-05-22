namespace QuilvianSystemBackendDev.Areas.Finance.AR.ViewModels
{
    public class ARCanceledViewModel
    {
        public Guid ARHeaderId { get; set; }

        public DateTime CanceledDate { get; set; }

        public string NoInvoice { get; set; } = string.Empty;

        public Guid CanceledOperatorId { get; set; }

        public string NamaCanceledOperator { get; set; } = string.Empty;

        public string CanceledReason { get; set; } = string.Empty;
    }
}
