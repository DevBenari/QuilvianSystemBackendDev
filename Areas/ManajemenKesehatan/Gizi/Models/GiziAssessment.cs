using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Gizi.Models
{
    public class GiziAssessment : UserActivity
    {
        [Key]
        public Guid AssessmentId { get; set; }          // PK, generate otomatis
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }

        // STRING DATA
        public string? Anthropometri { get; set; }
        public string? Biokimia { get; set; }
        public string? Klinis { get; set; }
        public string? RiwayatGizi { get; set; }
        public string? RiwayatPersonal { get; set; }
        public string? DiagnosisGizi { get; set; }
        public string? IntervensiGizi { get; set; }
        public string? JenisDiet { get; set; }
        public string? BentukMakanan { get; set; }
        public string? Frekuensi { get; set; }
        public string? RutePemberian { get; set; }

        // NUMERIC DATA
        public decimal? Energi { get; set; }
        public decimal? Protein { get; set; }
        public decimal? Karbohidrat { get; set; }
        public decimal? Lemak { get; set; }

        // TEXT FIELDS
        public string? EdukasiAwal { get; set; }
        public string? Keterangan { get; set; }

        // DATETIME
        public DateTime? TglPencatatan { get; set; }
    }
}
