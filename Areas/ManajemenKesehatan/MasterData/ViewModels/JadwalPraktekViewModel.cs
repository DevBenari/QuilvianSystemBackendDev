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
        public string JamMulai { get; set; }
        public string JamBerakhir { get; set; }
    }
}
