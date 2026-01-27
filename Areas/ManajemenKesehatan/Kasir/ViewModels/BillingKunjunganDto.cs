namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels
{
    public class BillingKunjunganDto
    {
        public DateTime? AsOf { get; set; }

        public Guid? KunjunganID { get; set; }
        public string? JenisKunjungan { get; set; }
        public DateTime? TanggalKunjungan { get; set; }
        public Guid? KasirId { get; set; }

        public string? NamaLengkap { get; set; }
        public string? NoRekamMedis { get; set; }
        public string? NmDokter { get; set; }
        public string? NamaPoliklinik { get; set; }
        public string? TipePembayaran { get; set; }
        public string? NamaAsuransi { get; set; }
        public string? Umur { get; set; }

        public List<object>? DaftarPemeriksaanLab { get; set; } = new();
        public List<object>? DaftarObat { get; set; } = new();
        public List<object>? DaftarRacikan { get; set; } = new();
        public List<object>? DaftarTindakan { get; set; } = new();
        public List<object>? DaftarBiayaAdmin { get; set; } = new();
        public List<object>? DaftarAlkes { get; set; } = new();

        public List<object>? DaftarVisitDokter { get; set; } = new();
        public List<object>? DaftarKamarRanap { get; set; } = new();

        public decimal? TotalPemeriksaanLab { get; set; }
        public decimal? TotalObat { get; set; }
        public decimal? TotalRacikan { get; set; }
        public decimal? TotalTindakan { get; set; }
        public decimal? TotalBiayaAdmin { get; set; }
        public decimal? TotalAlkes { get; set; }

        public decimal? TotalBiayaVisitDokter { get; set; }
        public decimal? TotalKamarRanap { get; set; }

        public decimal? TotalKeseluruhan { get; set; }
    }
}
