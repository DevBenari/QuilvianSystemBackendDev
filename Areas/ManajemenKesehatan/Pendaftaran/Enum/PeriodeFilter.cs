using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum
{
    [JsonConverter(typeof(StringEnumConverter))] // Agar dikonversi ke string saat di-serialize JSON
    public enum PeriodeFilter
    {
        [EnumMember(Value = "Hari Ini")]
        [Display(Name = "Hari Ini")]
        Today = 0,

        [EnumMember(Value = "Minggu Ini")]
        [Display(Name = "Minggu Ini")]
        ThisWeek = 1,

        [EnumMember(Value = "Minggu Lalu")]
        [Display(Name = "Minggu Lalu")]
        LastWeek = 2,

        [EnumMember(Value = "Bulan Ini")]
        [Display(Name = "Bulan Ini")]
        ThisMonth = 3,

        [EnumMember(Value = "Bulan Lalu")]
        [Display(Name = "Bulan Lalu")]
        LastMonth = 4,

        [EnumMember(Value = "Tahun Ini")]
        [Display(Name = "Tahun Ini")]
        ThisYear = 5,

        [EnumMember(Value = "Tahun Lalu")]
        [Display(Name = "Tahun Lalu")]
        LastYear = 6,

        [EnumMember(Value = "3 Bulan Terakhir")]
        [Display(Name = "3 Bulan Terakhir")]
        Last3Months = 7,

        [EnumMember(Value = "6 Bulan Terakhir")]
        [Display(Name = "6 Bulan Terakhir")]
        Last6Months = 8
    }
}
