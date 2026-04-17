using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstBarangStok", Schema = "public")]
    public class BarangStok : UserActivity
    {
        [Key]
        public Guid StokBarangId { get; set; }
        public Guid? BarangId { get; set; }
        public Guid? LokasiPenyimpananId { get; set; }
        public decimal? QtyStokBarang {  get; set; }
        public string? Keterangan {  get; set; }
    }
}
