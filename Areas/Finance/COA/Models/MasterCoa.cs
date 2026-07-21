using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.Finance.COA.Models
{
    [Table("Fin_MasterCoa", Schema = "public")]
    public class MasterCoa : UserActivity
    {
        [Key]
        public Guid COAId { get; set; }

        public Guid? GrupCOAId { get; set; }
        
        public Guid TipeAkunCOAId { get; set; }
        public Guid? CostCenterId { get; set; }

        public string? TipeTransaksi { get; set; }

        [MaxLength(200)]
        public string? NamaCOA { get; set; }

        [MaxLength(50)]
        public string? KodeCOA { get; set; }

        public bool? IsPostable { get; set; }
        public bool? IsValid { get; set; }
        public bool? IsPLACC { get; set; }

        [MaxLength(20)]
        public string? NomalBalance { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }
    }
}