using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models
{
    [Table("FarmasiRJ", Schema = "public")]
    public class FarmasiRJ : UserActivity
    {
        [Key]
        public Guid FarmasiRJId { get; set; }
        public Guid? ObatId { get; set; }
        public Guid? KonversiSatuanId { get; set; }
        public decimal? QtySatuan { get; set; }
        public decimal? QtyKonversi { get; set; }
        public string? BatchNumber { get; set; }
        public string? RackLocation { get; set; }
        public DateOnly? TanggalKadaluarsa { get; set; }
    }
}
