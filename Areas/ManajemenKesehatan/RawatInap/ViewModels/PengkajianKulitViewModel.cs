using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels
{
    public class PengkajianKulitViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? PengkajianPerawatId { get; set; }
        public bool? IsTerganggu { get; set; }
        public decimal? SkalaDekubitus { get; set; }
        [MaxLength(250)]
        public string? KondisiKulit { get; set; }
        public string? Keterangan { get; set; }
    }
}
