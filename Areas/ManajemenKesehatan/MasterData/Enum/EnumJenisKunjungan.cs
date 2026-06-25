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

        [Display(Name = "OPLab")]
        OPLab = 2,

        [Display(Name = "OPRad")]
        OPRad = 3,
    }
}
