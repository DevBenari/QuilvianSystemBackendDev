using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Finance.AR.Models
{
    [Table("Fin_ARCanceled", Schema = "public")]
    public class ARCanceled : UserActivity
    {
        [Key]
        public Guid ARCanceledId { get; set; }

        public Guid ARHeaderId { get; set; }

        public DateTime CanceledDate { get; set; }

        public string NoInvoice { get; set; } = string.Empty;

        public Guid CanceledOperatorId { get; set; }

        public string NamaCanceledOperator { get; set; } = string.Empty;

        public string CanceledReason { get; set; } = string.Empty;
    }
}
