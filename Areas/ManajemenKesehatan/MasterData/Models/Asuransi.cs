using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers;
using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstAsuransi", Schema = "public")]
    public class Asuransi : UserActivity
    {
        [Key]
        public Guid AsuransiId { get; set; }
        public string? KodeAsuransi { get; set; }

        // Informasi Asuransi
        public string? NamaAsuransi { get; set; }
        public string? JenisAsuransi { get; set; }
        public bool? StatusAsuransi { get; set; }

        public string? TanggalMulaiKerjasama { get; set; }
        public string? TanggalAkhirKerjasama { get; set; }

        // Informasi Klaim
        public decimal? CoveragePercentage { get; set; }
        public string? MetodeKlaim { get; set; }

        // Informasi Pertanggungan
        public int? TambahanTanggungan { get; set; }

        // Informasi Pembayaran
        public string? TermOfPayment { get; set; }

        // Informasi Kontak Utama
        public string? NamaPerusahaanAsuransi { get; set; }
        public string? EmailPusat { get; set; }
        public string? namaPIC { get; set; }
        public string? noPic { get; set; }
        public string? noVerificationAdmin { get; set; }
        public string? Alamat { get; set; }
        public string? Keterangan { get; set; }
        public bool? IsPKS { get; set; }
    }
}
