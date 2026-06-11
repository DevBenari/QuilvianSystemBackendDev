using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels
{
    public class CetakFilmViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public Guid? DokterPerujukId { get; set; }
        public Guid? KelasId { get; set; }
        public Guid? LabBookingId { get; set; }
        public Guid? HasilLabId { get; set; }
        public string? NoOrder { get; set; }
        public DateOnly? TglOrder { get; set; }
        public TimeOnly? WaktuOrder { get; set; }
        public DateTime? TglSelesai { get; set; }
        [Column(TypeName = "numeric")]
        public decimal? TotalCetakFilm { get; set; }
        public string? TipeLayanan {  get; set; }
        public string? Keterangan { get; set; }
        public List<CetakFilmDetailViewModel>? Details { get; set; }
    }
}
