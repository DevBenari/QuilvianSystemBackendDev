using System.Text.Json.Serialization;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class SubPoliViewModel
    {
        public Guid PoliId { get; set; }
        public string NamaSubPoli { get; set; }
        public string? Deskripsi { get; set; }
        public string KepalaSubPoli { get; set; }
        public string Lokasi { get; set; }
        public string Telepon { get; set; }
        public string Email { get; set; }
        public string HariOperasional { get; set; }

        [JsonConverter(typeof(TimeOnlyJsonConverter))]
        public TimeOnly? JamBuka { get; set; }
        [JsonConverter(typeof(TimeOnlyJsonConverter))]
        public TimeOnly? JamTutup { get; set; }
        public string? LayananSubPoli { get; set; }
        public int JumlahMaxPasien { get; set; }
    }
}
