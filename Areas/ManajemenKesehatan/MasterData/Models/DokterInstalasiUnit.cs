using QuilvianSystemBackendDev.Areas.HRD.MasterData.Models;
using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstDokterInstalasiUnit", Schema = "public")]
    public class DokterInstalasiUnit : UserActivity
    {
        [Key]
        public Guid DokterInstalasiUnitId { get; set; }

        public Guid DokterId { get; set; }

        public Guid InstalasiUnitId { get; set; }

        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(DokterId))]
        public Dokter? Dokter { get; set; }

        [ForeignKey(nameof(InstalasiUnitId))]
        public InstalasiUnit? InstalasiUnit { get; set; }
    }
}
