using System.Text.Json.Serialization;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Enum
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum StatusResepFilter
    {
        Diterima = 0,
        Diproses = 1,
        Selesai = 2,
    }
}