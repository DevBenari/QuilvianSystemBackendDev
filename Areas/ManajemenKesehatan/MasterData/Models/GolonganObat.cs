using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstGolonganObat", Schema = "public")]
    public class GolonganObat : UserActivity
    {
        [Key]
        public Guid GolonganObatId { get; set; }
        public string? NamaGolonganObat { get; set; }
        public string? Keterangan { get; set; }
    }
}
