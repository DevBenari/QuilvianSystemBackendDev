using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.Finance.COA.Models
{

    [Table("Fin_TipeAkun", Schema = "public")]
    public class TipeAkun : UserActivity
    {
        [Key]
        public Guid TipeAkunCOAId { get; set; }

        [MaxLength(200)]
        public string? NamaTipeAkunCOA { get; set; }

        public decimal? KodeTipeAkunCOA { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }
    }
}
