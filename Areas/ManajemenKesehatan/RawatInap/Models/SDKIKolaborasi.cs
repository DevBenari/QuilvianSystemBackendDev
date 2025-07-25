using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    [Table("SDKIKolaborasi", Schema = "public")]
    public class SDKIKolaborasi : UserActivity
    {
        [Key]
        public Guid SDKIKolaborasiId { get; set; }
        public Guid? SDKIEtiologiId { get; set; }
        public string? NamaKolaborasi { get; set; }
        public string? Keterangan { get; set; }
    }
}
