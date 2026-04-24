using System.Text.Json.Serialization;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Enum
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FilterStatusSuratPengantarRanap
    {
        Menunggu = 0,
        Selesai = 1,
        Dibatalkan = 2,
        Kadaluarsa = 3,
    }
}
