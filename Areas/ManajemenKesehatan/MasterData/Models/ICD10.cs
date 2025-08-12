using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstICD-10", Schema = "public")]
    public class ICD10 : UserActivity
    {
        [Key]
        public Guid ICDId { get; set; }
        public string? ICDCode { get; set; }
        public string? ICDName { get; set; }
        public string? DTDCode { get; set; }
        public string? NamaDtd { get; set; }
    }
}
