using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models
{
    [Table("MstRacikanBentuk", Schema ="public")]
    public class RacikanBentuk : UserActivity
    {
        [Key]
        public Guid? BentukRacikanId { get; set; }
        public string? LatinBentukRacikan {  get; set; }
        public string? NamaBentukRacikan { get; set; }
        public string? Keterangan {  get; set; }

        public ICollection<Racikan> Racikans { get; set; } = new HashSet<Racikan>();

    }
}
