using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers;
using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstAsuransi", Schema = "public")]
    public class Asuransi : UserActivity
    {
        [Key]
        public Guid? AsuransiId { get; set; }
        public string? KodeAsuransi { get; set; }
        public DateTimeOffset? Createdate { get; set; }

        // Informasi Asuransi
        public string? NamaAsuransi { get; set; }
        public string? JenisAsuransi { get; set; }
        public string? KategoriAsuransi { get; set; }
        public string? StatusAsuransi { get; set; }
        public DateTimeOffset? TanggalMulaiKerjasama { get; set; }
        public DateTimeOffset? TanggalAkhirKerjasama { get; set; }

        // Informasi Klaim
        public string? MetodeKlaim { get; set; }
        public int? BatasMaxKlaimPerTahun { get; set; }
        public int? BatasMaxKlaimPerKunjungan { get; set; }

        // Informasi Pertanggungan
        public int? PersentasiBiayaPertanggungan { get; set; }
        public int? TambahanTanggungan { get; set; }

        // Informasi Pembayaran
        public string? NoRekRumahSakit { get; set; }
        public string? NamaBank { get; set; }
        public string? TermOfPayment { get; set; }

        // Informasi Kontak Utama
        public string? NamaPerusahaanAsuransi { get; set; }
        public string? NoTelepon { get; set; }
        public string? EmailPusat { get; set; }
        public bool? IsPKS { get; set; }

        public ICollection<CoveranAsuransi> CoveranAsuransis { get; set; }
    }
}
