using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.Finance.COA.Models
{
    [Table("Fin_GrupCoa", Schema = "public")]
    public class GrupCoa : UserActivity
    {
        [Key]
        public Guid GrupCOAId { get; set; }

        [MaxLength(200)]
        public string? NamaGrupCOA { get; set; }

        [MaxLength(50)]
        public string? KodeGrupCOA { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }
        [MaxLength(200)]
        public string? test { get; set; }
    }
}
