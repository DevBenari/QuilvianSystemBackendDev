using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstTarifFilm")]
    public class TarifFilm : UserActivity
    {
        [Key]
        public Guid? TarifFilmId { get; set; }
        public Guid? FilmId { get; set; }
        public Guid? KelasId { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? TarifDokter { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? TarifRs { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? TarifJp { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? TarifBahp { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? TarifLain { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? TarifTotal { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? KSO { get; set; }
        public string? Keterangan { get; set; }

        // =========================
        // Navigation Property
        // =========================

        [ForeignKey(nameof(FilmId))]
        public Film? Film { get; set; }

        [ForeignKey(nameof(KelasId))]
        public Kelas? Kelas { get; set; }
    }
}
