using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstKomoditas", Schema = "public")]
    public class Komoditas : UserActivity
    {
        [Key]
        public Guid KomoditasId { get; set; }
        public string? NamaKomoditas { get; set; }
        public bool? IsMaterialGrup { get; set; }
        public bool? IsKomoditas { get; set; }
        public string? Keterangan { get; set; }
    }
}
