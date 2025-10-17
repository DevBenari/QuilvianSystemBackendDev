using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.HRD.Pengajuan.Models
{
    [Table("Hrd_CounterOffer", Schema = "public")]
    public class CounterOffer : UserActivity
    {
        [Key]
        public Guid CounterOfferId { get; set; }
        public Guid UserActiveId { get; set; }
        public string? PerusahaanRekruter { get; set; }
        public string? IndustriRekruter { get; set; }
        public string? TawaranJabatan { get; set; }
        public DateTime? TglOffer { get; set; }
        public decimal? TawaranGaji { get; set; }
        public decimal? InsentifPercentase { get; set; }
        public decimal? TawaranKompensasi { get; set; }
        public DateTime? DeadlineRespont { get; set; }
        public string? TawaranBenefitFasilitas { get; set; }
        public decimal? UsulGaji { get; set; }
        public decimal? PercentaseKenaikan { get; set; }
        public decimal? PercentaseBonus { get; set; }
        public string? EquityPenyesuaian { get; set; }
        public DateTime? TglEfektif { get; set; }
        public string? PermintaanPromosi { get; set; }
        public string? PermintaanLainnya { get; set; }
        public string? PencapaianUtama { get; set; }
        public string? RisetPasar { get; set; }
        public string? KomitmenMasaDepan { get; set; }
        public string? LevelRisk { get; set; }
        public string? KnowladgeTransferRisk { get; set; }
    }
}
