using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    public class DokterPoli : UserActivity
    {
        [Key]
        public Guid DokterPoliId { get; set; }
        public Guid DokterId { get; set; }
        public Guid? PoliId { get; set; }
        public string NamaDokter { get; set; }
        public string? KodeDokterPoli { get; set; }
        public string? KodeDokterSubPoli { get; set; }
        public Guid? SubPoliId { get; set; }
        public string? NamaPoliKlinik { get; set; }
        public string? NamaSubPoli { get; set; }

        public ICollection<JadwalPraktek> JadwalPraktek { get; set; }

        // Relasi ke Dokter
        [ForeignKey("DokterId")]
        public Dokter? Dokter { get; set; }

        // Relasi ke Poliklinik
        [ForeignKey("PoliId")]
        public Poliklinik? Poliklinik { get; set; }

        // Relasi ke SubPoli
        [ForeignKey("SubPoliId")]
        public SubPoli? SubPoli { get; set; }

    }
}
