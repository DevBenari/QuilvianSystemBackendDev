using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Finance.Faktur.Models
{
    [Table("FIN_TukarFaktur", Schema = "public")]
    public class TukarFaktur : UserActivity
    {
        public Guid TukarFakturId { get; set; }
        public Guid SupplierId { get; set; }

        public DateTime TglRegistrasi { get; set; }
        public DateTime? TglTerimaFaktur { get; set; }
        public DateTime? TglJatuhTempo { get; set; }

        public string Keterangan { get; set; }
    }
}
