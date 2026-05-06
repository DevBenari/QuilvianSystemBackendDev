using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models
{
    [Table("MasterDenominasi", Schema = "public")]
    public class MasterDenominasi : UserActivity
    {
        [Key]
        public Guid DenominasiId { get; set; }
        public string KodeDenominasi { get; set; } = string.Empty;
        public decimal MataUang { get; set; }
        public decimal NominalPecahan { get; set; }
        public string? Keterangan { get; set; }
        public bool? IsDelete { get; set; } = false;
    }
}