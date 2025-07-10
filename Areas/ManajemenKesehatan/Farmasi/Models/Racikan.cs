using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models
{
    [Table("MstRacikan", Schema = "public")]
    public class Racikan : UserActivity
    {
        [Key]
        public Guid RacikanId { get; set; }
        public string? NamaRacikan { get; set; }
        public string? Keterangan { get; set; }
        public string? Signa { get; set; }
        public string? SignaTambahan { get; set; } 
        public int? QtyRacikan { get; set; }
        public string? KodeRacikan { get; set; }
    }
}
