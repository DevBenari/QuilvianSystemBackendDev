using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels
{
    public class MainKasirTebusResepViewModel
    {
        [Required]
        public Guid? ResepTebusId { get; set; }

        public bool? IsVerified { get; set; }
        public Guid? TTDUserVerfiedId { get; set; }

        public decimal? GrandTotalPembayaran { get; set; }
        public bool? StatusPembayaran { get; set; }
        public string? StatusBilling { get; set; }
        public string? Keterangan { get; set; }

        public List<MainKasirDetailViewModel>? Details { get; set; }
    }
}
