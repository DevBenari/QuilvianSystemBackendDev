using System.Text.Json.Serialization;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Enum
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FilterStatusSuratPengantarRanap
    {
        Menunggu = 0,
        Proses = 1,
        Selesai = 2,
        Dibatalkan = 3,
        Kadaluarsa = 4,
    }
}
