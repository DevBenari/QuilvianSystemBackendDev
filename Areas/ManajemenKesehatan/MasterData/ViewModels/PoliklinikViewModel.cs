using System.Text.Json.Serialization;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class PoliklinikViewModel
    {
        public string NamaPoliklinik { get; set; }
        public string KepalaPoliklinik { get; set; }
        public string Lokasi { get; set; }
        public string Telepon { get; set; }
        public string Email { get; set; }
        public string HariOperasional { get; set; }

        [JsonConverter(typeof(TimeOnlyJsonConverter))]
        public TimeOnly? JamBuka { get; set; }

        [JsonConverter(typeof(TimeOnlyJsonConverter))]
        public TimeOnly? JamTutup { get; set; }
        public string LayananPoliklinik { get; set; }
        public int JumlahMaxPasien { get; set; }
        public string Deskripsi { get; set; }
    }
}
