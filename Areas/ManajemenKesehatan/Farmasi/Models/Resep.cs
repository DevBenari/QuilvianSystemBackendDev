using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models
{
    [Table("MstResep", Schema = "public")]
    public class Resep : UserActivity
    {
        [Key]
        public Guid ResepId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? AsuransiId { get; set; }
        public string? NamaAsuransi { get; set; }
        public Guid? PasienId { get; set; }
        public string? NamaPasien { get; set; }
        public Guid? PoliklinikId { get; set; }
        public string? NamaPoliklinik { get; set; }
        public Guid? DokterId { get; set; }
        public string? NamaDokter { get; set; }
        public int? AntrianResep { get; set; }
        public string? AntrianRegistrasi { get; set; }
        public string? StatusPembuatanResep { get; set; }
        public bool? StatusPengambilanResep { get; set; } = false;
        public bool? IsCancelled { get; set; } = false;
        public bool? IsLunas { get; set; }
        public DateTime? TanggalPembuatanResep { get; set; }
    }
}
