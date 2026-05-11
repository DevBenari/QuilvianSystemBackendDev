using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.Finance.COA.Models
{
    [Table("Fin_MasterGrup", Schema = "public")]
    public class MasterGrup : UserActivity
    {
        [Key]
        public Guid GrupCOAId { get; set; }

        public Guid? TipeAkunCOAId { get; set; }

        [MaxLength(200)]
        public string? NamaGrupCOA { get; set; }

        [MaxLength(50)]
        public string? KodeGrupCOA { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }
    }
}
