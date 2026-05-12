using MessagePack;
using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Finance.AR.Models
{
    [Table("FIN_ARSettlement", Schema = "public")]
    public class ARSettlement : UserActivity
    {
        public Guid SettlementARId { get; set; }

        public Guid KunjunganId { get; set; }

        public Guid PasienId { get; set; }

        public string NamaPasien { get; set; } = string.Empty;

        public string NoInvoice { get; set; } = string.Empty;

        public decimal BeginingBalance { get; set; }

        public decimal EndingBalance { get; set; }
    }
}
