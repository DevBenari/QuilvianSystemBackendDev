using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class JadwalPraktekViewModel
    {
        public Guid DokterId { get; set; }
        public Guid DokterPoliId { get; set; }
        public string NamaDokter { get; set; }
        public Guid? PoliId { get; set; } // Bisa null jika praktek di SubPoli
        public Guid? SubPoliId { get; set; } // Bisa null jika praktek di Poli
        public string WaktuPraktek { get; set; } //pagi siang sore malam
        public string HariPraktek { get; set; }
        public DateTime? JamMulai { get; set; }
        public DateTime? JamBerakhir { get; set; }
    }
}
