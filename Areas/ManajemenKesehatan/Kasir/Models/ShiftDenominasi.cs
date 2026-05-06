using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models
{
    [Table("ShiftDenominasi", Schema = "public")]
    public class ShiftDenominasi : UserActivity
    {
        [Key]
        public Guid ShiftDenominasiId { get; set; }
        public string KodeShiftDenominasi { get; set; } = string.Empty;
        public Guid LayananId { get; set; }
        public Guid KasirId { get; set; }
        public string TipePerhitungan { get; set; } = string.Empty; // Buka / Tutup
        public Guid DenominasiId { get; set; }
        public decimal LembarKoin { get; set; }
        public decimal TotalDenominasi { get; set; }
        public string? Keterangan { get; set; }
        public bool? IsDelete { get; set; } = false;
    }
}