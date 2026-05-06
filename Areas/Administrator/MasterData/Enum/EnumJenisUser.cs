using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace QuilvianSystemBackendDev.Areas.Administrator.MasterData.Enum
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EnumJenisUser
    {
        [Display(Name = "Admin")]
        Admin = 0,

        [Display(Name = "Dokter")]
        Dokter = 1,

        [Display(Name = "IT Support")]
        ITSupport = 2,

        [Display(Name = "Perawat")]
        Perawat = 3,

        [Display(Name = "Apoteker")]
        Apoteker = 4,

        [Display(Name = "Teknisi Medis")]
        TeknisiMedis = 5,

        [Display(Name = "Administrasi")]
        Administrasi = 6,

        [Display(Name = "Manajemen")]
        Manajemen = 7,

        [Display(Name = "Guest")]
        Guest = 8,

        [Display(Name = "Customer Service")]
        CustomerService = 9,

        [Display(Name = "Direksi")]
        Direksi = 9
    }
}

