using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstBarangHarga", Schema = "public")]
    public class BarangHarga : UserActivity
    {
        [Key]
        public Guid HargaBarangId { get; set; }

        // FK ke MasterBarang
        public Guid? BarangId { get; set; }

        public decimal? HteHargaBarang { get; set; }
        public decimal? HneHargaBarang { get; set; }
        public DateTime? TglBerlaku { get; set; }
        public DateTime? TglBerakhir { get; set; }
        public string? Keterangan { get; set; }
    }
}
