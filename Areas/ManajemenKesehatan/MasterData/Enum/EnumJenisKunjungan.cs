using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Enum
{

    public enum EnumJenisKunjungan
    {
        [Display(Name = "IP")]
        IP = 0,

        [Display(Name = "OP")]
        OP = 1,
    }
}
