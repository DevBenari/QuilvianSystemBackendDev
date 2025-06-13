using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstKonversiSatuan", Schema = "public")]
    public class KonversiSatuan : UserActivity
    {
        [Key]
        public Guid? KonversiSatuanId { get; set; }
        public Guid? ObatId { get; set; }
        public Guid? SatuanId { get; set; }
        public string? NamaSatuan { get; set; }
        public string? TipeKonversi { get; set; } // e.g., "Pcs", "Box", "Botol"
        public decimal? NilaiKonversi { get; set; }
    }
}
