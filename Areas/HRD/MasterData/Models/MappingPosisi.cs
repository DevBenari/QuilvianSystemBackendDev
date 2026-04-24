using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.Models
{
    [Table("Hrd_MappingPosisi", Schema = "public")]
    public class MappingPosisi : UserActivity
    {
        [Key]
        public Guid MappingPosisiId { get; set; }
        public Guid? DepartementId { get; set; }
        public Guid? InstalasiUnitId { get; set; }
        public string? Keterangan {  get; set; }
    }
}
