using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class DokterPraktekViewModel
    {
        public string Dokter { get; set; }
        public string Layanan { get; set; }
        public string JamPraktek { get; set; }
        public string Hari { get; set; }
        public DateTime? JamMasuk { get; set; }
        public DateTime? JamKeluar { get; set; }

        public Guid DokterId { get; set; }
    }
}
