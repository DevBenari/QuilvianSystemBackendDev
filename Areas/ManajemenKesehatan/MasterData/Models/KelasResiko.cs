using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstKelasResiko", Schema = "public")]
    public class KelasResiko : UserActivity
    {
        [Key]
        public Guid KelasResikoId { get; set; }
        public string? KodeKelasResiko {  get; set; }
        public string? NamaKelasResiko { get; set; }
        public string? Keterangan {  get; set; }
    }
}
