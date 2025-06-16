using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.Keuangan.Kasir.Models
{
    [Table("MetodePembayaran", Schema = "public")]
    public class MetodePembayaran : UserActivity
    {
        [Key]
        public Guid MetodePembayaranId { get; set; }
        public string? NamaMetode { get; set; }
        public string? Keterangan { get; set; }
        public bool? IsDelete { get; set; }
    }
}
