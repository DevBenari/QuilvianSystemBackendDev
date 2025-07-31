using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Enum
{
    public enum DelegasiTugas
    {
        [Display(Name = "Vital Sign")]
        VitalSign = 0,

        [Display(Name = "Pain Assessment")]
        PainAssessment = 1,
    }
}
