using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    [Table("SDKITeraupetik", Schema = "public")]
    public class SDKITeraupetik : UserActivity
    {
        [Key]
        public Guid SDKITeraupetikId { get; set; }
        public Guid? SDKIDiagnosaId { get; set; }
        public string? NamaTeraupetik { get; set; }
        public string? Keterangan { get; set; }
    }
}
