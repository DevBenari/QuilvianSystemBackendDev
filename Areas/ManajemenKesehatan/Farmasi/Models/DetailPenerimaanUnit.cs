using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models
{
    public class DetailPenerimaanUnit : UserActivity
    {
        [Key]
        public Guid DetailPenerimaanUnitId { get; set; }
        public Guid? PenerimaanUnitId { get; set; }
        public Guid? ObatId { get; set; }
        public decimal? QtyPermintaan { get; set; }
        public string? SatuanItem { get; set; }
        public string? KategoriItem { get; set; }
        public string? Keterangan { get; set; }
    }
}
