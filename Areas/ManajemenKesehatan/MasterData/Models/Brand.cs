using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstBrand", Schema = "public")]
    public class Brand : UserActivity
    {
        [Key]
        public Guid BrandId { get; set; }
        public string? KodeBrand { get; set; }
        public Guid? SupplierId { get; set; }
        public string? NamaBrand { get; set; }
        public string? Keterangan {  get; set; }
    }
}
