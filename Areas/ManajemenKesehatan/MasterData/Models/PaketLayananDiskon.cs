using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    public class PaketLayananDiskon : UserActivity
    {
        [Key]
        public Guid DiskonPaketLayananId { get; set; }
        public string? KodeDiskonPaket { get; set; }
        public Guid? PaketLayananId { get; set; }
        public Guid? PaketLayananAsuransiId { get; set; }
        public Guid? DiskonPercentageId { get; set; }
        public decimal? PotonganHargaMax { get; set; }
        public DateTime? PeriodeAwal { get; set; }
        public DateTime? PeriodeAkhir { get; set; }
        public string? Keterangan { get; set; }
    }
}
