using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models
{
    [Table("MainKasir", Schema = "public")]
    public class MainKasir : UserActivity
    {
        [Key]
        public Guid KasirId { get; set; }
        public Guid? KunjunganId { get; set; }
        public decimal? GrandTotalPembayaran { get; set; }
        public decimal? TotalBiayaObat { get; set; }
        public decimal? TotalBiayaTindakan { get; set; }
        public string? Keterangan { get; set; }
        public DateTimeOffset? TglPembayaran { get; set; }
        public Guid? DiskonId { get; set; }
    }

}
