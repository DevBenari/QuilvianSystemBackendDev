using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstKategoriTerapeutik", Schema = "public")]
    public class KategoriTerapeutik : UserActivity
    {
        [Key]
        public Guid KategoriTerapeutikId { get; set; }
        public string? NamaKategoriTerapeutik { get; set; }
        public string? FungsiObat { get; set; }
        public string? Keterangan { get; set; }
    }
}
