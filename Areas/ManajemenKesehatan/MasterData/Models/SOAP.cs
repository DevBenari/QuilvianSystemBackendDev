using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstSOAP", Schema = "public")]
    public class SOAP : UserActivity
    {
        [Key]
        public Guid SOAPID { get; set; }
        public Guid? KunjunganId { get; set; }
        public string? Subjective { get; set; }
        public string? Objective { get; set; }
        public string? DaftarICD10 { get; set; } // dengan ICD-10
        public string? DaftarSDKI { get; set; } 
        public string? Assessment { get; set; }
        public string? Planning { get; set; }
        public string? Evaluasi { get; set; }
        public string? Intervensi { get; set; }
        public string? Reevaluasi { get; set; }
        public string? Profesi { get; set; }
    }
}
