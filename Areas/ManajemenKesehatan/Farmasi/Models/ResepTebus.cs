using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models
{
    [Table("ResepTebus", Schema = "public")]
    public class ResepTebus : UserActivity
    {
        [Key]
        public Guid ResepTebusId { get; set; }
        public int? AntrianResep { get; set; }
        public string? NamaPenebus { get; set; }
        public string? StatusPembuatanResep { get; set; }
        public bool? StatusPengambilan { get; set; } = false;
        public bool? IsCancelled { get; set; } = false;
        public bool? IsLunas { get; set; }
        public Guid? GudangUnitId { get; set; }
        public Guid? InstalasiUnitId { get; set; }
        public string? JenisLayanan { get; set; }
        public decimal? TotalHargaResep { get; set; }
        public DateTime? TanggalLunas { get; set; }
        public Guid? PetugasFarmasiId { get; set; }
        public string? NoResepLuar { get; set; }
        public string? AsalFaskes { get; set; }
        public string? NoHpPenebus { get; set; }
        public DateTime? TanggalPembuatanResep { get; set; }
    }
}
