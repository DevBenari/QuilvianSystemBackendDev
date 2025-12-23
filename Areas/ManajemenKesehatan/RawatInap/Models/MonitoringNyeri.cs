using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    public class MonitoringNyeri : UserActivity
    {
        [Key]
        public Guid MonitoringNyeriId { get; set; }       // Generate otomatis
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }

        public DateTime? WaktuMonitoringNyeri { get; set; }

        public decimal? SkorNyeri { get; set; }
        public decimal? SkorSedasi { get; set; }

        public decimal? Sistolik { get; set; }
        public decimal? Diastolic { get; set; }
        public decimal? Nadi { get; set; }
        public decimal? Respirasi { get; set; }
        public decimal? Suhu { get; set; }

        public Guid? PerawatMonitoringId { get; set; }
        public string? ParafPerawatMonitoring { get; set; }

        public DateTime? WaktuIntervensi { get; set; }
        public Guid? ObatId { get; set; }
        public string? Dosis { get; set; }
        public string? Rute { get; set; }
        public string? IntervensiNonFarmakologi { get; set; }

        public Guid? PerawatIntervensiId { get; set; }
        public string? ParafPerawatIntervensi { get; set; }

        public DateTime? WaktuKajianUlang { get; set; }
        public string? Keterangan { get; set; }
    }
}
