using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    public class MstSubBakteri : UserActivity
    {
        [Key]
        public Guid SubBakteriId { get; set; }
        public string? NamaSubBakteri { get; set; }

        public string? KodeSubBakteri { get; set; }

        public string? Keterangan { get; set; }


        // =========================
        // Navigation Property
        // =========================
        public ICollection<MstMappingBakteri> MapBakteris { get; set; }
            = new List<MstMappingBakteri>();

        public ICollection<MstMapAntibiotikSubBakteri> MapAntibiotikSubBakteris { get; set; }
            = new List<MstMapAntibiotikSubBakteri>();
    }
}
