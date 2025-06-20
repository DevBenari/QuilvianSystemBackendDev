namespace QuilvianSystemBackendDev.Areas.Keuangan.Kasir.ViewModels;

public class MainKasirViewModel
{
    public Guid? KunjunganId { get; set; }
    public string? BiayaAdministrasiKode { get; set; }
    public Guid? MetodePembayaranId { get; set; }
    public Guid? DiskonId { get; set; }
    public decimal? NominalPembayaran { get; set; }
    public decimal? TotalBiayaObat { get; set; }
    public decimal? TotalBiayaTindakan { get; set; }
    public string? StatusPembayaran { get; set; }
    public string? Keterangan { get; set; }
    public Guid? ReferenceId { get; set; }
}
