using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Enum
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EnumJenisKunjungan
    {
        [Display(Name = "IP")]
        IP = 0,

        [Display(Name = "OP")]
        OP = 1,
    }
}
