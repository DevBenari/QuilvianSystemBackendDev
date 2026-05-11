using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.Administrator.MasterData.Models
{
    [Table("Hrd_MasterBank", Schema = "public")]
    public class MasterBank : UserActivity
    {
        [Key]
        public Guid BankId { get; set; }

        [MaxLength(200)]
        public string? BankName { get; set; }

        [MaxLength(100)]
        public string? BankShortName { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }
    }
}
