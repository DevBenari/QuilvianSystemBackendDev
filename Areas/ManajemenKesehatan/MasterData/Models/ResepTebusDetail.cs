using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("ResepTebusDetail", Schema = "public")]
    public class ResepTebusDetail : UserActivity
    {
        [Key]
        public Guid ResepTebusDetailId { get; set; }
        public Guid? ResepTebusId { get; set; }
        public Guid? RacikanId { get; set; }
        public bool? IsRacikan { get; set; }
        public Guid? ObatId { get; set; }
        public int? Qty { get; set; }
        public string? Signa { get; set; }
        public string? SignaTambahan { get; set; }
        public decimal? HargaObat { get; set; }
    }
}
