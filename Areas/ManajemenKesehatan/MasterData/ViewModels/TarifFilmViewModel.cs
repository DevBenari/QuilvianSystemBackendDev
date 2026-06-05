using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class TarifFilmViewModel
    {
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
    }
}
