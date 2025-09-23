using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.Administrator.MasterData.Models
{
    // ======================================
    // 1. JENIS USER
    // ======================================
    [Table("JnsUser", Schema = "public")]
    public class JenisUser
    {
        [Key]
        public Guid JenisUserId { get; set; }

        [Required]
        public string NamaJenisUser { get; set; } = string.Empty;
        public string? Kode { get; set; }
        public int? No { get; set; }
        public string? Tlp { get; set; }
        public string? Pas { get; set; }
        public string? Keterangan { get; set; }
        public string? Status { get; set; }
        public int KodePembayaran { get; set; }
    }
    // ======================================
    // 2. JENIS PEMBAYARAN
    // ======================================
    [Table("JnsPembayaran", Schema = "public")]
    public class JenisPembayaran
    {
        [Key]
        public Guid JenisPembayaranId { get; set; }

        [Required]
        public int KodePembayaran { get; set; }
        public string NamaPembayaran { get; set; }
        public int NominalDefault { get; set; }
        public string? Keterangan { get; set; }
        public string JenisTanggal { get; set; }
        public string? TanggalMasuk { get; set; }
        public string? TanggalKeluar { get; set; }
        public string? Set { get; set; }
        public string? Status { get; set; }
    }
    // ======================================
    // 3. PEMBAYARAN
    // ======================================
    [Table("JnsPembayaranNominal", Schema = "public")]
    public class Pembayaran
    {
        [Key]
        public Guid PembayaranId { get; set; }

        [Required]
        public Guid JenisUserId { get; set; }
        public string NamaJenisUser { get; set; } = string.Empty;

        [Required]
        public Guid JenisPembayaranId { get; set; }
        public int KodePembayaran { get; set; }
        public string NamaPembayaran { get; set; } = string.Empty;
        public int Nominal { get; set; }

        [Column(TypeName = "date")]
        public DateTime TanggalPembayaran { get; set; }
        public string? Keterangan { get; set; }
        public string? Status { get; set; }
    }
}
