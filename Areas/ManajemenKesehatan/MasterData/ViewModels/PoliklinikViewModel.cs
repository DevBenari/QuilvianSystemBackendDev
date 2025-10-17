using System.Text.Json.Serialization;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class PoliklinikViewModel
    {
        public string NamaPoliklinik { get; set; }
        public string KepalaPoliklinik { get; set; }
        public string Ruang { get; set; }
        public string Telepon { get; set; }
        public string Email { get; set; }
        public string HariOperasional { get; set; }

        public TimeSpan? JamBuka { get; set; }
        public TimeSpan? JamTutup { get; set; }

        public string LayananPoliklinik { get; set; }
        public int JumlahMaxPasien { get; set; }
        public string Deskripsi { get; set; }
    }
}
