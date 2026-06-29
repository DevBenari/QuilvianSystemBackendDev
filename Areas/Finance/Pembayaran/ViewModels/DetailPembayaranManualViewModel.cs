using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.Finance.Pembayaran.ViewModels
{
    public class DetailPembayaranManualViewModel
    {
        [Required]
        public Guid PembayaranManualId { get; set; }

        [Required]
        public Guid CoaId { get; set; }

        [Required]
        [MaxLength(500)]
        public string? DeskripsiPembayaran { get; set; }

        [Required]
        public Guid CostCenterId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Nominal pembayaran harus lebih dari 0.")]
        public decimal NominalPembayaran { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }
    }
}