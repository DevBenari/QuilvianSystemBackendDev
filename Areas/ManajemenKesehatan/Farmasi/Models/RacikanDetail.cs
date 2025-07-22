using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models
{
    [Table("RacikanDetail", Schema = "public")]
    public class RacikanDetail : UserActivity
    {
        [Key]
        public Guid DetailRacikanId { get; set; }
        public Guid? RacikanId { get; set; }
        public Guid? ObatId { get; set; }
        public int? QtyUsed { get; set; }
        public decimal? KomposisiDosis { get; set; }
        public decimal? HargaKomposisi { get; set; }
    }
}
