using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models
{
    public class DetailPermintaanUnit : UserActivity
    {
        [Key]
        public Guid DetailPermintaanUnitId { get; set; }
        public Guid? PermintaanUnitId { get; set; }
        public Guid? ObatId { get; set; }
        public decimal? QtyPermintaan { get; set; }
        public string? SatuanItem { get; set; }
        public string? KategoriItem { get; set; }
        public string? Keterangan { get; set; }

        // navigation
        public PermintaanUnit? PermintaanUnit { get; set; }
        public Obat? Obat { get; set; }
    }
}
