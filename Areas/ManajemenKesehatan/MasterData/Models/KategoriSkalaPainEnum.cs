using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    public enum KategoriSkalaPainEnum
    {
        [Display(Name = "Numeric (Sadar dan Bisa Komunikasi)")]
        Numeric = 0,

        [Display(Name = "Wong-Baker (Anak-anak > 3 tahun)")]
        WongBaker=1,

        [Display(Name = "Comfort (Tidak Sadar/Bayi)")]
        Comfort=2
    }
}
