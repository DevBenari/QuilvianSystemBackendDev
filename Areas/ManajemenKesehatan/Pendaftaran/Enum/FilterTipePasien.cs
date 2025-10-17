using System.Text.Json.Serialization;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TipePasienFilter
    {
        Umum = 0,
        Rujukan = 1
    }
}
