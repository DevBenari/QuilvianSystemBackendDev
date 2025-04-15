using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class SkalaPainViewModel
    {
        public Guid? KunjunganId { get; set; }
        public string? ScoreSkalaPain { get; set; }
        public string? Deskripsi { get; set; }
        
        [Required]
        public KategoriSkalaPainEnum KategoriSkalaEnum { get; set; }

        // ✅ Untuk disimpan ke database jika dibutuhkan, auto dari enum
        [NotMapped]
        public string KategoriSkala => KategoriSkalaEnum.ToString();

        // ✅ Untuk tampilan label (mengambil [Display(Name = "...")])
        [NotMapped]
        public string KategoriSkalaDisplay =>
            KategoriSkalaEnum.GetType()
            .GetField(KategoriSkalaEnum.ToString())
            ?.GetCustomAttributes(typeof(DisplayAttribute), false)
            is DisplayAttribute[] attrs && attrs.Length > 0
                ? attrs[0].Name
                : KategoriSkalaEnum.ToString();
    }
}
