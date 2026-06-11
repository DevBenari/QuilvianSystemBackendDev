using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models
{
    [Table("CetakFilmDetail")]
    public class CetakFilmDetail : UserActivity
    {
        [Key]
        public Guid DetailCetakFilmId { get; set; } // Generate otomatis

        public Guid? CetakFilmId { get; set; }

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


        // =========================
        // Navigation Property
        // =========================

        public CetakFilm? CetakFilm { get; set; }

        public LabHasilDetail? LabHasilDetail { get; set; }

        public LabBookingDetail? LabBookingDetail { get; set; }

        public Lab? Lab { get; set; }

        public LabPemeriksaan? Pemeriksaan { get; set; }

        public Dokter? DokterPemeriksa { get; set; }

        public Film? Film { get; set; }
    }
}
