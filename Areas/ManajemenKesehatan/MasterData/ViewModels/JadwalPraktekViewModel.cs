using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class JadwalPraktekViewModel
    {
        public Guid DokterPoliId { get; set; }
        public string WaktuPraktek { get; set; } //pagi siang sore malam
        public string HariPraktek { get; set; }

        [JsonConverter(typeof(TimeOnlyJsonConverter))]
        public TimeOnly? JamMulai { get; set; }
        [JsonConverter(typeof(TimeOnlyJsonConverter))]
        public TimeOnly? JamBerakhir { get; set; }
    }
}
