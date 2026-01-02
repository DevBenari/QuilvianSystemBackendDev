using Newtonsoft.Json;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels
{
    public class ObatViewModel
    {
        public string ObatName { get; set; }
        public string? JumlahSatuan { get; set; }
        public Guid? SatuanId { get; set; }
        public Guid BentukSatuanId { get; set; }
        public decimal HTEPrice { get; set; }
        public decimal? HNAPrice { get; set; }
        public bool? IsActive { get; set; }
        public int Stock { get; set; }
        public int? Minimal { get; set; }
        public int? Maximal { get; set; }
        public string? Farmakologi { get; set; }
        public string? Peringatan { get; set; }
        public string? Indikasi { get; set; }
        public string? Kontraindikasi { get; set; }
        public string? CaraKerja { get; set; }
        public decimal? TakaranDosis { get; set; }
        public string? Dosis { get; set; }
        public string? InteraksiObat { get; set; }
        public string? Note { get; set; }
        public string? Kategori { get; set; }
        public Guid? ItemId { get; set; }
        public Guid? ObatRuteId { get; set; }
        public string? KategoriObat { get; set; }
        public bool? IsControlled { get; set; }

        //public string KategoriObat { get; set; }
        //public string Asuransi { get; set; }
        //public string KandunganObat { get; set; }
        //public string TipeHarga { get; set; } 
    }

    public class ObatKandunganViewModel
    {
        public Guid ObatId { get; set; }
        public Guid KandunganId { get; set; }
    }
    public class ObatAsuransiViewModel
    {
        public Guid ObatId { get; set; }
        public Guid AsuransiId { get; set; }
        // ============================
        // MARKUP
        // ============================
        public decimal? MarkupDokter { get; set; }
        public decimal? MarkupRs { get; set; }
        public decimal? MarkupJp { get; set; }
        public decimal? MarkupBahp { get; set; }
        public decimal? MarkupLainnya { get; set; }
        public decimal? MarkupTotal { get; set; }

        public bool IsMarkupBerlaku { get; set; } = true;

        // Tahun Bulan (misal: 2025-01)
        public DateTime? MarkupDari { get; set; }
        public DateTime? MarkupSampai { get; set; }


        // ============================
        // DISKON
        // ============================
        public decimal? DiskonDokter { get; set; }
        public decimal? DiskonRs { get; set; }
        public decimal? DiskonJp { get; set; }
        public decimal? DiskonBahp { get; set; }
        public decimal? DiskonTotal { get; set; }

        public bool IsDiskonBerlaku { get; set; } = true;

        // Tahun Bulan (misal: 2025-01)
        public DateTime? DiskonDari { get; set; }
        public DateTime? DiskonSampai { get; set; }
    }
}
