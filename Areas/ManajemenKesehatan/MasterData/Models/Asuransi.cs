using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstAsuransi", Schema = "public")]
    public class Asuransi : UserActivity
    {
        [Key]
        //informasi utama
        public Guid? AsuransiId { get; set; }
        public string? KodeAsuransi { get; set; }
        public DateTimeOffset? Createdate { get; set; }

        //informasi Asuransi
        public string? NamaAsuransi { get; set; }
        public string? JenisAsuransi { get; set; }
        public string? KategoriAsuransi { get; set; }
        public string? StatusAsuransi { get; set; }
        public DateTimeOffset? TanggalMulaiKerjasama { get; set; }
        public DateTimeOffset? TanggalAkhirKerjasama { get; set; }
        public string? RSRekanan { get; set; }

        // informasi klaim
        public string? MetodeKlaim { get; set; }
        public DateTimeOffset? WaktuKlaim { get; set; }
        public int? BatasMaxKlaimPerTahun { get; set; }
        public int? BatasMaxKlaimPerKunjungan { get; set; }
        public string? DokumenKlaim { get; set; }

        // informasi pertanggungan
        public string? Layanan { get; set; }
        public int? PersentasiBiayaPertanggungan { get; set; }
        public string? ObatDitanggung { get; set; }
        public int? TambahanTanggungan { get; set; }
        public int? BiayaTidakDitanggung { get; set; }
        public int? MasaTunggu { get; set; }
        public int? MaxUsiaPasien { get; set; }

        //informasi pembayaran
        public string? NoRekRumahSakit { get; set; }
        public string? NamaBank { get; set; }
        public string? NamaBankCabang { get; set; }
        public string? TermOfPayment { get; set; }
        public DateTime? BatasWaktuPembayaran { get; set; }
        public int? PenaltiTerlambatBayar { get; set; }

        //informasi kontak dan dukungan
        public string? NamaPerusahaanAsuransi { get; set; }
        public string? AlamatPusat { get; set; }
        public string? AlamatCabang { get; set; }
        public string? NoTelepon { get; set; }
        public string? EmailPusat { get; set; }
        public string? NoHotlineDarurat { get; set; }

        //informasi perwakilan asuransi
        public string? NamaPerwakilan { get; set; }
        public string? NoTeleponPerwakilan { get; set; }
        public string? EmailPerwakilan { get; set; }
        public string? JabatanPerwakilan { get; set; }
    }
}
