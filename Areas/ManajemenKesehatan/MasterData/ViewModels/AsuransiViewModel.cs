using System.Text.Json.Serialization;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class AsuransiViewModel
    {
        public DateOnly? Createdate { get; set; }

        // Informasi Asuransi
        public string? NamaAsuransi { get; set; }
        public string? JenisAsuransi { get; set; }
        public string? KategoriAsuransi { get; set; }
        public string? StatusAsuransi { get; set; }
        public DateTime? TanggalMulaiKerjasama { get; set; }
        public DateTime? TanggalAkhirKerjasama { get; set; }
        public string? RSRekanan { get; set; }
        public bool IsPKS { get; set; }

        // Informasi Klaim
        public string? MetodeKlaim { get; set; }

        public DateTime? WaktuKlaim { get; set; }

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
    }
}
