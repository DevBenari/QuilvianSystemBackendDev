using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstGudang", Schema = "public")]
    public class Gudang : UserActivity
    {
        [Key]
        public Guid GudangId { get; set; }
        public string? NamaGudang { get; set; }
        public string? Lokasi { get; set; }
        public string? Keterangan { get; set; }
    }
}
