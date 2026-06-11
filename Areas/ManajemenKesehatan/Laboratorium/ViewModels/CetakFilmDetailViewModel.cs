using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels
{
    public class CetakFilmDetailViewModel
    {
        public Guid? DetailHasilLabId { get; set; }
        public Guid? LabBookingDetailId { get; set; }
        public Guid? LabId { get; set; }
        public Guid? PemeriksaanId { get; set; }
        public string? NamaPemeriksaan { get; set; }
        public string? NoPhoto { get; set; }
        public Guid? DokterPemeriksaId { get; set; }
        public string? NamaDokterPemeriksa { get; set; }
        public string? PathHasilPhoto { get; set; }
        public string? HasilLab { get; set; }
        public string? HasilLabAI { get; set; }
        public Guid? FilmId { get; set; }
        [Column(TypeName = "numeric")]
        public decimal? HargaSatuanFilm { get; set; }
        [Column(TypeName = "numeric")]
        public decimal? QtyCetakFilm { get; set; }
        [Column(TypeName = "numeric")]
        public decimal? TotalCetakFilm { get; set; }
        public string? Keterangan { get; set; }
    }
}
