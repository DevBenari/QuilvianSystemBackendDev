using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstObatKandungan", Schema = "public")]
    public class ObatKandungan : UserActivity
    {
        [Key]
        public Guid ObatKandunganId { get; set; }
        public Guid ObatId { get; set; }
        public Guid KandunganId { get; set; }
    }
}
