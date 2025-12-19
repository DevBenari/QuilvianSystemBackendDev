using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models
{
    [Table("MstObatRute", Schema = "public")]
    public class ObatRute : UserActivity
    {
        [Key]
        public Guid? RuteObatId { get; set; }
        public string? RuteObat {  get; set; }
        public string? Keterangan {  get; set; }
    }
}
