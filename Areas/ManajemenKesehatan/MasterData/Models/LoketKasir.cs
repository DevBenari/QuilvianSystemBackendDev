using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstLoketKasir", Schema = "public")]
    public class LoketKasir : UserActivity
    {
        [Key]
        public Guid LoketKasirId { get; set; }
        public Guid? LayananId { get; set; }
        public string? NamaLoket {  get; set; }
        public string? LantaiKe {  get; set; }
        public string? Keterangan {  get; set; }
    }
}
