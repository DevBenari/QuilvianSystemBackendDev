using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    public class MstMapAntibiotikSubBakteri : UserActivity
    {
        [Key]
        public Guid MapAntibiotikSubBakteriId { get; set; }

        public Guid? SubBakteriId { get; set; }

        public Guid? AntibiotikId { get; set; }

        public decimal? NormalMin { get; set; }

        public decimal? NormalMax { get; set; }

        public decimal? CriticalMin { get; set; }

        public decimal? CriticalMax { get; set; }

        public decimal? UrutanTampil { get; set; }

        public string? Keterangan { get; set; }


        // =========================
        // Navigation Property
        // =========================
        public MstSubBakteri? SubBakteri { get; set; }

        public MstAntibiotik? Antibiotik { get; set; }
    }
}
