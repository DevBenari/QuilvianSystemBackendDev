using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstTermOfPayment", Schema = "public")]
    public class TermOfPayment : UserActivity
    {
        [Key]
        public Guid TermOfPaymentId { get; set; }
        public string TermOfPaymentCode { get; set; }
        public string TermOfPaymentName { get; set; }
        public string? Note { get; set; }
    }
}
