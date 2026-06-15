using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Finance.MasterFinance.Models
{
    [Table("FIN_ExchangeRate", Schema = "public")]
    public class ExchangeRate : UserActivity
    {
        [Key]
        public Guid ExchangeRateId { get; set; }

        public Guid MataUangId { get; set; }

        [NotMapped]
        public string? KodeMataUang { get; set; }

        [NotMapped]
        public string? NamaMataUang { get; set; }

        [Column(TypeName = "numeric(18,6)")]
        public decimal RateToIDR { get; set; }

        public DateTime RateDate { get; set; }

        public string? Keterangan { get; set; }
    }
}
