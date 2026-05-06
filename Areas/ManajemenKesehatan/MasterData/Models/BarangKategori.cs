using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstBarangKategori", Schema = "public")]
    public class BarangKategori : UserActivity
    {
        [Key]
        public Guid KategoriBarangId { get; set; }
        public string? KodeKategoriBarang {  get; set; }
        public string? NamaKategoriBarang { get; set; }
        public string? GrupKategoriBarang { get; set; }
        public string? Keterangan {  get; set; }
    }
}
