namespace QuilvianSystemBackendDev.Areas.Finance.Pembayaran.ViewModels
{
    public class CanceledReceivedPaymentViewModel
    {
        public Guid? ReceivedPaymentId { get; set; }
        public string? NoRef { get; set; }
        public Guid? CancelledOperatorId { get; set; }
        public string? CancelReason { get; set; }
    }
}
