namespace QuilvianSystemBackendDev.Areas.Keuangan.Kasir.ViewModels;

public class MainKasirViewModel
{
    public Guid? KunjunganId { get; set; }
    public string? BiayaAdministrasiKode { get; set; }
    public Guid? MetodePembayaranId { get; set; }
    public Guid? DiskonId { get; set; }
    public string? StatusPembayaran { get; set; }
    public string? Keterangan { get; set; }
    //public DateTimeOffset? TglPembayaran { get; set; }
    public Guid? ReferenceId { get; set; }
}
