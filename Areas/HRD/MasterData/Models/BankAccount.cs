using QuilvianSystemBackendDev.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.Models
{
    [Table("Hrd_BankAccount", Schema = "public")]
    public class BankAccount : UserActivity
    {
        [Key]
        public Guid BankAccountId { get; set; }

        [MaxLength(50)]
        public string? BankId { get; set; }

        [MaxLength(200)]
        public string? BankName { get; set; }

        [MaxLength(100)]
        public string? BankShortName { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? NoAccount { get; set; }

        [MaxLength(200)]
        public string? AccountName { get; set; }

        [MaxLength(10)]
        public string? CurrencyCode { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }
    }
}