using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;

public class MainKasirViewModel
{
    [Required]
    public Guid? KunjunganId { get; set; }
    public string? BiayaAdministrasiKode { get; set; }
    public Guid? DiskonId { get; set; }
    public decimal? GrandTotalPembayaran { get; set; }
    public decimal? TotalBiayaObat { get; set; }
    public decimal? TotalBiayaTindakan { get; set; }
    public string? Keterangan { get; set; }
    public List<MainKasirDetailViewModel> Details { get; set; }
}
