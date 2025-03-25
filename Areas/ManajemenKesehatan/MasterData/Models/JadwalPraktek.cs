using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstJadwalPraktek", Schema = "public")]
    public class JadwalPraktek : UserActivity
    {
        [Key]
        public Guid JadwalPraktekId { get; set; }
        public Guid? DokterPoliId { get; set; }
        public string KodeJadwalPraktek { get; set; }
        public string WaktuPraktek { get; set; } 
        public string HariPraktek { get; set; }

        [JsonConverter(typeof(TimeOnlyJsonConverter))]
        public TimeOnly? JamMulai { get; set; }

        [JsonConverter(typeof(TimeOnlyJsonConverter))]
        public TimeOnly? JamBerakhir { get; set; }

        [ForeignKey("DokterPoliId")]
        public DokterPoli DokterPoli { get; set; }


    }
}
