using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("KamarAsuransi", Schema = "public")]
    public class KamarAsuransi : UserActivity
    {
        [Key]
        public Guid KamarAsuransiId { get; set; }
        public Guid? KamarId { get; set; }
        public Guid? AsuransiId { get; set; }
        public decimal? MarkupRs { get; set; }
        public decimal? MarkupJp { get; set; }
        public decimal? MarkupBahp { get; set; }
        public decimal? MarkupLainnya { get; set; }
        public decimal? MarkupTotal { get; set; }

        public bool IsMarkupBerlaku { get; set; } = true;

        // Tahun Bulan (misal: 2025-01)
        public DateTime? MarkupDari { get; set; }
        public DateTime? MarkupSampai { get; set; }
        public string? Keterangan { get; set; }

    }
}
