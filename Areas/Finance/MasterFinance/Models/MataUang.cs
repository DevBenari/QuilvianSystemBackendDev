using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.Finance.MasterFinance.Models
{
    [Table("Fin_MataUang", Schema = "public")]
    public class MataUang : UserActivity
    {
        [Key]
        public Guid MataUangId { get; set; }

        [Required]
        [MaxLength(10)]
        public string KodeMataUang { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string NamaMataUang { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? SimbolMataUang { get; set; }

        public bool IsBaseCurrency { get; set; }

        public string? Keterangan { get; set; }
    }
}
