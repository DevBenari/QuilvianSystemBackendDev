using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Enum
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum StatusBayarEnum
    {
        [Display(Name = "Lunas")]
        Lunas = 0,

        [Display(Name = "Cicil")]
        Cicil = 1, 
        
        [Display(Name = "Belum Bayar")]
        BelumBayar = 2,
    }
}
