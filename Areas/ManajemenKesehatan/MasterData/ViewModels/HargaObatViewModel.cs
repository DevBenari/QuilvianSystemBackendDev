namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class HargaObatViewModel
    {
        public Guid? ItemId { get; set; } // Relasi dengan master item
        public string? Currency { get; set; } // Contoh: IDR, USD, dll
        public decimal? HargaHNA { get; set; } // Harga Neto Apotek
        public decimal? HargaHTE { get; set; } // Harga Tertinggi Eceran
        public bool? IsTermasukPajak { get; set; } // True jika termasuk pajak
        public DateTime? AwalEfektif { get; set; } // Tanggal mulai efektif
        public DateTime? AkhirEfektif { get; set; } // Tanggal akhir efektif
        public string? Keterangan { get; set; } // Catatan tambahan
    }
}
