using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.Models
{
    [Table("Hrd_InstalasiUnit", Schema = "public")]
    public class InstalasiUnit : UserActivity
    {
        [Key]
        public Guid InstalasiUnitId { get; set; }
        public string? KodeInstalasiUnit {  get; set; }
        public string? NamaInstalasiUnit { get; set; }
        public Guid? DepartementId { get; set; }
        public string? Keterangan {  get; set; }
    }
}
