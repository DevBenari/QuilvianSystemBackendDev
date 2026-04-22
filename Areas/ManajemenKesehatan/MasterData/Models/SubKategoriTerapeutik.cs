using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstSubKategoriTerapeutik", Schema = "public")]
    public class SubKategoriTerapeutik:UserActivity
    {
        [Key]
        public Guid SubKategoriTerapeutikId { get; set; }
        public Guid? KategoriTerapeutikId { get; set; }
        public string? NamaSubKategoriTerapeutik { get; set; }
        public string? FungsiObat { get; set; }
        public string? Keterangan { get; set; }
    }
}
