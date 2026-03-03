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
        public bool? IsPKS { get; set; }
        public string? Umur { get; set; }

        public List<object>? DaftarPemeriksaanLab { get; set; } = new();
        public List<object>? DaftarObat { get; set; } = new();
        public List<object>? DaftarRacikan { get; set; } = new();
        public List<object>? DaftarTindakan { get; set; } = new();
        public List<object>? DaftarBiayaAdmin { get; set; } = new();
        public List<object>? DaftarAlkes { get; set; } = new();

        public List<object>? DaftarVisitDokter { get; set; } = new();
        public List<object>? DaftarKamarRanap { get; set; } = new();
        public object? DPRanap { get; set; } = 0m;

        public decimal TotalPemeriksaanLab { get; set; } = 0m;
        public decimal TotalObat { get; set; } = 0m;
        public decimal TotalRacikan { get; set; } = 0m;
        public decimal TotalTindakan { get; set; } = 0m;
        public decimal TotalBiayaAdmin { get; set; } = 0m;
        public decimal TotalAlkes { get; set; } = 0m;

        public decimal? SubTotalMandiri { get; set; } = 0m;
        public decimal? SubTotalAsuransi { get; set; } = 0m;

        public decimal TotalBiayaVisitDokter { get; set; } = 0m;
        public decimal TotalKamarRanap { get; set; } = 0m;
        public decimal TotalDPRanap { get; set; } = 0m;
        public decimal TotalKeseluruhan { get; set; } = 0m;
    }
}
