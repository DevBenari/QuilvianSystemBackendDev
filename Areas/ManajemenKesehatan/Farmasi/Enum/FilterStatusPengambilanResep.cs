using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Enum
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum StatusPengambilanResepFilter
    {
        Belum = 0,

        [Display(Name = "Tidak diambil")]
        Tidak = 1,

        [Display(Name = "Diambil sebagian")]
        Sebagian = 2,

        [Display(Name = "Diambil semua")]
        Semua = 3,
    }
}
