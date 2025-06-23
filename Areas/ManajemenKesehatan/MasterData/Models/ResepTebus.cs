using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("ResepTebus", Schema = "public")]
    public class ResepTebus : UserActivity
    {
        [Key]
        public Guid ResepTebusId { get; set; }
        public int? AntrianResep { get; set; }
        public string? StatusPembuatanResep { get; set; }
        public bool? StatusPengambilan { get; set; } = false;
        public bool? IsCancelled { get; set; } = false;
        public bool? IsLunas { get; set; }
        public DateOnly? TanggalPembuatanResep { get; set; }

    }
}
