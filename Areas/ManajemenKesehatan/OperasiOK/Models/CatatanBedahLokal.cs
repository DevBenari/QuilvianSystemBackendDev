using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.Models
{
    public class CatatanBedahLokal : UserActivity
    {
        [Key]
        public Guid CatBedahLokalId { get; set; }
        public Guid? CatBedahId { get; set; }
        public string? KomplikasiAkut {  get; set; }
        public string? TemuanSaatOperasi {  get; set; }
        public string? Pengawasan {  get; set; }
        public string? Kontrol {  get; set; }
        public string? Terapi {  get; set; }
        public string? Keterangan { get; set; }
    }
}
