using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    [Table("SDKIGroup", Schema = "public")]
    public class SDKIGroup : UserActivity
    {
        [Key]
        public Guid SDKIGroupId { get; set; }
        public string? NamaGroupSDKI { get; set; }
        public string? Keterangan { get; set; }
    }
}
