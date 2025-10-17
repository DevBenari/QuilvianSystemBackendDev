using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    [Table("PerawatSubjective", Schema = "public")]
    public class PerawatSubjective : UserActivity
    {
        [Key]
        public Guid SubNurseId { get; set; }
        public Guid? DiagnosaSDKIId { get; set; }
        public string? NamaSubjective { get; set; }
        public string? Keterangan { get; set; }
    }
}
