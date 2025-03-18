using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstJadwalPraktek", Schema = "public")]
    public class JadwalPraktek : UserActivity
    {
        [Key]
        public Guid JadwalPraktekId { get; set; }
        public Guid? DokterPoliId { get; set; }
        public string WaktuPraktek { get; set; } //pagi siang sore malam
        public string HariPraktek { get; set; }
        public TimeSpan? JamMulai { get; set; }
        public TimeSpan? JamBerakhir { get; set; }

    }
}
