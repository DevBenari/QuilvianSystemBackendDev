using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    public class TarifKamar : UserActivity
    {
        [Key]
        public Guid TarifKamarId { get; set; }
        public Guid? KamarId { get; set; }
        public Guid? KelasId { get; set; }
        public decimal? TarifDokter { get; set; }
        public decimal? TarifRs { get; set; }
        public decimal? TarifJp { get; set; }
        public decimal? TarifBahp { get; set; }
        public decimal? TarifLain { get; set; }
        public decimal? TarifTotal { get; set; }
        public decimal? KSO { get; set; }
        public string? Keterangan { get; set; }
    }
}
