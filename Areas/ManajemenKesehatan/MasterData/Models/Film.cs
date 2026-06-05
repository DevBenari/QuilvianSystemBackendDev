using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstFilm")]
    public class Film : UserActivity
    {
        [Key]
        public Guid FilmId { get; set; }
        public string? NamaFilm { get; set; } = string.Empty;
        public string? UkuranFilm { get; set; }
        public string? Keterangan { get; set; }

        // =========================
        // Navigation Property
        // =========================

        public ICollection<TarifFilm> TarifFilms { get; set; } = new List<TarifFilm>();
    }
}
