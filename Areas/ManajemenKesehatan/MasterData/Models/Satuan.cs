using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstSatuan", Schema = "public")]
    public class Satuan : UserActivity
    {
        [Key]
        public Guid SatuanId { get; set; }
        public string? KodeSatuan { get; set; }
        public string? NamaSatuan { get; set; }
        public string? SingkatanSatuan { get; set; }
        public string? EnSatuan { get; set; }

        public ICollection<Obat> Obats { get; set; } = new HashSet<Obat>();
    }
}
