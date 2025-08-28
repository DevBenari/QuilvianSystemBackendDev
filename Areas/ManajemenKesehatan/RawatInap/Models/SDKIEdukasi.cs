using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    [Table("SDKIEdukasi", Schema = "public")]
    public class SDKIEdukasi : UserActivity
    {
        [Key]
        public Guid SDKIEdukasiId { get; set; }
        public Guid? SDKIDiagnosaId { get; set; }
        public string? NamaEdukasi { get; set; }
        public string? Keterangan { get; set; }
    }
}
