using QuilvianSystemBackendDev.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.Models
{
    [Table("Hrd_HasilTest", Schema = "public")]
    public class HasilTest : UserActivity
    {
        [Key]
        public Guid HasilTestId { get; set; }

        [MaxLength(200)]
        public string? NamaPeserta { get; set; }

        public decimal? NomorPeserta { get; set; }

        public DateTimeOffset? TglTest { get; set; }

        [MaxLength(200)]
        public string? HasilTestText { get; set; }
    }
}
