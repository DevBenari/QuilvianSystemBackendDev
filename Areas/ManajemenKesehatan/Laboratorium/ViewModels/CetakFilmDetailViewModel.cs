using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels
{
    public class CetakFilmDetailViewModel
    {
        public Guid? CetakFilmId { get; set; }
        public Guid? DetailHasilLabId { get; set; }
        public Guid? LabBookingDetailId { get; set; }
        public Guid? DokterPemeriksaId { get; set; }
        public Guid? FilmId { get; set; }
        [Column(TypeName = "numeric")]
        public decimal? QtyCetakFilm { get; set; }
        [Column(TypeName = "numeric")]
        public decimal? TotalCetakFilm { get; set; }
        public string? Keterangan { get; set; }
    }
}
