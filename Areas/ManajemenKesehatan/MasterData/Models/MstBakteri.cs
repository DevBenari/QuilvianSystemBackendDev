using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    public class MstBakteri : UserActivity
    {
        [Key]
        public Guid BakteriId { get; set; }

        public string? KodeBakteri { get; set; }

        public string? Keterangan { get; set; }


        // =========================
        // Navigation Property
        // =========================
        public ICollection<MstMappingBakteri> MapBakteris { get; set; }
            = new List<MstMappingBakteri>();
    }
}
