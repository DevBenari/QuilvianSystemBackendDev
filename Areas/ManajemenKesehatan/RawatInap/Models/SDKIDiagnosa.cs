using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    [Table("SDKIDiagnosa", Schema = "public")]
    public class SDKIDiagnosa : UserActivity
    {
        [Key]
        public Guid SDKIDiagnosaId { get; set; }
        public Guid? SDKIDiagnosaGroupId { get; set; }
        public string? SDKIKodeDiagnosa { get; set; }
        public string? NamaDiagnosa { get; set; }
        public string? Keterangan { get; set; }
    }
}
