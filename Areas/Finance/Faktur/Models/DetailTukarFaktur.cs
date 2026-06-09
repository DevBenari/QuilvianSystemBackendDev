using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Finance.Faktur.Models
{
    [Table("FIN_DetailTukarFaktur", Schema = "public")]
    public class DetailTukarFaktur : UserActivity
    {
        public Guid DetailTukarFakturId { get; set; }
        public Guid TukarFakturId { get; set; }

        public string NomorPO { get; set; }
        public string NoInvoice { get; set; }

        public decimal TotalInvoice { get; set; }

        public string Keterangan { get; set; }
    }
}
