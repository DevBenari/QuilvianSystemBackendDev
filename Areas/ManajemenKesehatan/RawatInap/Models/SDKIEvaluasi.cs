using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    [Table("SDKIEvaluasi", Schema = "public")]
    public class SDKIEvaluasi : UserActivity
    {
        [Key]
        public Guid SDKIEvaluasiId { get; set; }
        public Guid? SDKIDiagnosaId { get; set; }
        public string? NamaEvaluasi { get; set; }
        public string? Keterangan { get; set; }

    }
}
