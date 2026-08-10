using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    public class MstAntibiotik : UserActivity
    {
        [Key]
        public Guid AntibiotikId { get; set; }

        public string? KodeAntibiotik { get; set; }

        public decimal? Microgram { get; set; }

        public string? Keterangan { get; set; }


        // =========================
        // Navigation Property
        // =========================
        public ICollection<MstMapAntibiotikSubBakteri> MapAntibiotikSubBakteris { get; set; }
            = new List<MstMapAntibiotikSubBakteri>();
    }
}
