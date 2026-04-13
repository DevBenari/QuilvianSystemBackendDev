using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstLokasiPenyimpanan", Schema = "public")]
    public class LokasiPenyimpanan : UserActivity
    {
        [Key]
        public Guid LokasiPenyimpananId { get; set; }
        public string? KodeLokasiPenyimpanan {  get; set; }
        public string? NamaLokasi { get; set; }
        public Guid? LantaiId {  get; set; }
        public string? Keterangan {  get; set; }
    }
}
