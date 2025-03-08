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
        public Guid DokterId { get; set; }
        public Guid? DokterPoliId { get; set; }
        public string NamaDokter { get; set; }
        public Guid? PoliId { get; set; } // Bisa null jika praktek di SubPoli
        public Guid? SubPoliId { get; set; } // Bisa null jika praktek di Poli
        public string KodeJadwalPraktek { get; set; }
        public string WaktuPraktek { get; set; } //pagi siang sore malam
        public string HariPraktek { get; set; }
        public DateTime? JamMulai { get; set; }
        public DateTime? JamBerakhir { get; set; }

        [ForeignKey("DokterPoliId")]
        public DokterPoli DokterPoli { get; set; }

        [ForeignKey("DokterSubPoliId")]
        public DokterSubPoli DokterSubPoli { get; set; }


    }
}
