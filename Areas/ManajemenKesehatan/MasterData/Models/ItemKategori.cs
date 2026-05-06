using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstItemKategori", Schema = "public")]
    public class ItemKategori : UserActivity
    {
        [Key]
        public Guid KategoriItemId { get; set; }
        public string? NamaKategoriItem { get; set; }
        public string? Keterangan {  get; set; }
    }
}
