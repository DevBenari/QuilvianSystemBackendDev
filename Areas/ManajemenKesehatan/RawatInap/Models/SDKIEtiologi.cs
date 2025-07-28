using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    [Table("SDKIEtiologi", Schema = "public")]
    public class SDKIEtiologi : UserActivity
    {
        [Key]
        public Guid SDKIEtiologiId { get; set; }
        public Guid? SDKIDiagnosaId { get; set; }
        public string? NamaEtiologi { get; set; }
        public string? Keterangan { get; set; }
    }
}
