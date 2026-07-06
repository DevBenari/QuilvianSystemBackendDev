using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    public class Kamar : UserActivity
    {
        [Key]
        public Guid KamarId { get; set; }
        public Guid? KelasId { get; set; }
        public string? KodeKamar { get; set; }
        public string? NamaKamar { get; set; }
        public decimal? TarifHarian { get; set; }
        public string? Lantai { get; set; }
        public string? PosisiRuangan { get; set; }
        public string? Deskripsi { get; set; }

        // navigation
        public Kelas? Kelas { get; set; }
    }
}
