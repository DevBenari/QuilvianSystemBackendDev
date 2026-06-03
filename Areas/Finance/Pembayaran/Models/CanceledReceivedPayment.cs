using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.Finance.Pembayaran.Models
{
    [Table("Fin_CanceledReceivedPayment", Schema = "public")]
    public class CanceledReceivedPayment : UserActivity
    {
        [Key]
        public Guid? CancelledReceivedPaymentId { get; set; }
        public Guid? ReceivedPaymentId { get; set; }
        public string? NoRef { get; set; }
        public Guid? CancelledOperatorId { get; set; }
        public string? CancelReason { get; set; }
    }
}
