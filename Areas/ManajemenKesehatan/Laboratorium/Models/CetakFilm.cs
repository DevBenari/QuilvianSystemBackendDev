using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models
{
    [Table("CetakFilm")]
    public class CetakFilm : UserActivity
    {
        [Key]
        public Guid CetakFilmId { get; set; } // Generate otomatis

        public Guid? KunjunganId { get; set; }

        public Guid? PasienId { get; set; }

        public Guid? DokterPerujukId { get; set; }

        public Guid? KelasId { get; set; }

        // OrderId / relasi ke LabBooking
        public Guid? LabBookingId { get; set; }

        public Guid? HasilLabId { get; set; }

        public string? NoOrder { get; set; }

        public DateOnly? TglOrder { get; set; }

        public TimeOnly? WaktuOrder { get; set; }

        public DateTime? TglSelesai { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? TotalCetakFilm { get; set; }

        public string? Keterangan { get; set; }


        // =========================
        // Navigation Property
        // =========================

        public Kunjungan? Kunjungan { get; set; }

        public PendaftaranPasienBaru? Pasien { get; set; }

        public Dokter? DokterPerujuk { get; set; }

        public Kelas? Kelas { get; set; }

        public LabBooking? LabBooking { get; set; }

        public LabHasil? LabHasil { get; set; }

        public ICollection<CetakFilmDetail> Details { get; set; } = new HashSet<CetakFilmDetail>();
    }
}
