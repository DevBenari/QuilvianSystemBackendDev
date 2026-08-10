using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    public class MstMappingBakteri : UserActivity
    {
        [Key]
        public Guid MapBakteriId { get; set; }

        public Guid? BakteriId { get; set; }

        public Guid? SubBakteriId { get; set; }

        public string? Keterangan { get; set; }


        // =========================
        // Navigation Property
        // =========================
        public MstBakteri? Bakteri { get; set; }

        public MstSubBakteri? SubBakteri { get; set; }

    }
}
