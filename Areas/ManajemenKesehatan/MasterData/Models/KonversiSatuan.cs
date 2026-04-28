using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstKonversiSatuan", Schema = "public")]
    public class KonversiSatuan : UserActivity
    {
        [Key]
        public Guid? KonversiSatuanId { get; set; }
        public Guid? ObatAlkesId { get; set; }
        public Guid? SatuanBesarId { get; set; }
        public Guid? SatuanKecilId { get; set; }
        public decimal? NilaiKonversi { get; set; }

        // navigation
        public ICollection<FarmasiRJ> FarmasiRJs { get; set; } = new List<FarmasiRJ>();
    }
}
