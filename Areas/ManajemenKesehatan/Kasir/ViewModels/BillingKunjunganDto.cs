namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels
{
    public class BillingKunjunganDto
    {
        public DateTime? AsOf { get; set; }

        public Guid? PasienId { get; set; }
        public Guid? KunjunganID { get; set; }
        public string? JenisKunjungan { get; set; }
        public string? AsalKunjungan { get; set; }
        public bool? IsClosed { get; set; }
        public DateTime? TanggalKunjungan { get; set; }
        public Guid? KasirId { get; set; }

        public string? NamaLengkap { get; set; }
        public string? NoHP { get; set; }
        public string? NoRekamMedis { get; set; }
        public string? NmDokter { get; set; }
        public string? NamaPoliklinik { get; set; }
        public string? TipePembayaran { get; set; }
        public string? NamaAsuransi { get; set; }
        public string? NoPolis {  get; set; }
        public bool? IsPKS { get; set; }
        public string? Umur { get; set; }

        // List Item Pelayanan Medis
        public List<object>? DaftarPemeriksaanLab { get; set; } = new();
        public List<object>? DaftarObat { get; set; } = new();
        public List<object>? DaftarRacikan { get; set; } = new();
        public List<object>? DaftarTindakan { get; set; } = new();
        public List<object>? DaftarBiayaAdmin { get; set; } = new();
        public List<object>? DaftarBiayaLain { get; set; } = new();
        public List<object>? DaftarAlkes { get; set; } = new();
        public List<object>? DaftarVisitDokter { get; set; } = new();
        public List<object>? DaftarKamarRanap { get; set; } = new();
        public object? DPRanap { get; set; } = 0m;

        // List Item Diskon
        public List<object>? DaftarPaketDiskon { get; set; } = new();

        // List Item Diskon Dokter / FoC
        public List<object>? DaftarDiskonDokter { get; set; } = new();
        
        public decimal TotalPemeriksaanLab { get; set; } = 0m;
        public decimal TotalObat { get; set; } = 0m;
        public decimal TotalRacikan { get; set; } = 0m;


        #region Total Tindakan + Tindakan FOC 
        // Total tindakan
        public decimal TotalTindakan { get; set; } = 0m;

        // Total tindakan normal + FoC
        public decimal TotalTindakanKeseluruhan { get; set; } = 0m;

        // Total FoC / Diskon Dokter keseluruhan
        public decimal TotalDiskonDokter { get; set; } = 0m;

        // Total FoC yang masuk asuransi
        public decimal TotalDiskonDokterAsuransi { get; set; } = 0m;

        // Total FoC yang masuk asuransi excess
        public decimal TotalDiskonDokterAsuransiExcess { get; set; } = 0m;

        // Optional: Total FoC mandiri
        public decimal TotalDiskonDokterMandiri { get; set; } = 0m;
        #endregion

        public decimal TotalBiayaAdmin { get; set; } = 0m;
        public decimal TotalBiayaLain { get; set; } = 0m;
        public decimal TotalAlkes { get; set; } = 0m;

        public decimal? SubTotalMandiri { get; set; } = 0m;
        public decimal? PajakTotalMandiri { get; set; } = 0m;
        public decimal? SebelumTaxTotalMandiri { get; set; } = 0m;
        public decimal? SubTotalAsuransi { get; set; } = 0m;
        public decimal? SubTotalAsuransiExcess { get; set; } = 0m;
        public decimal PPN => 11m;

        public decimal TotalBiayaVisitDokter { get; set; } = 0m;
        public decimal TotalKamarRanap { get; set; } = 0m;
        public decimal TotalKeseluruhan { get; set; } = 0m;

        // deposit
        public decimal? TotalSaldoDeposito { get; set; } = 0m;
        public decimal? NominalMasuk { get; set; } = 0m;
        public decimal? NominalKeluar { get; set; } = 0m;
    }
}
