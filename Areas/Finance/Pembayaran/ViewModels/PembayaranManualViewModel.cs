using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.Finance.Pembayaran.ViewModels
{
    public class PembayaranManualViewModel
    {
        public DateTime? TglDokumen { get; set; }

        public DateTime? TglPembayaranManual { get; set; }

        public Guid? MataUangId { get; set; }

        public Guid? ExchangeRateId { get; set; }

        public DateTime? TglJatuhTempo { get; set; }

        [Required]
        public Guid SupplierId { get; set; }

        public Guid? PajakId { get; set; }

        public decimal? PersenanPajak { get; set; }

        public decimal? NominalPajak { get; set; }

        [MaxLength(100)]
        public string? NomorFakturPajak { get; set; }

        public DateTime? TglFakturPajak { get; set; }

        public Guid? PoId { get; set; }

        [MaxLength(100)]
        public string? NoReferensiManual { get; set; }

        [MaxLength(100)]
        public string? StatusPembayaranManual { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }
    }
}