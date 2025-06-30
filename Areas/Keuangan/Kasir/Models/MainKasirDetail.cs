using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.Keuangan.Kasir.Models
{
    [Table("MainKasirDetail", Schema = "public")]
    public class MainKasirDetail : UserActivity
    {
        [Key]
        public Guid MainKasirDetailId { get; set; }
        public Guid? MainKasirId { get; set; }
        public Guid? MetodePembayaranId { get; set; }
        public Guid? ReferenceId { get; set; }
        public string? NamaMetode { get; set; }
        public decimal? NominalPembayaran { get; set; }
        public string? Keterangan { get; set; }
        public bool? StatusPembayaran { get; set; }
        public DateTime? TglPembayaran { get; set; }
    }
}
