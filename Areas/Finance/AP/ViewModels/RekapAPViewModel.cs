using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.Finance.AP.ViewModels
{
    public class RekapAPViewModel
    {
        [Required]
        public Guid SupplierId { get; set; }

        public decimal? RekapVariasiHarga { get; set; }

        public decimal? RekapOther { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }
    }
}