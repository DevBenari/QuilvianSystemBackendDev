namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class PPNViewModel
    {
        public string? NamaPpn { get; set; } = default!;
        public decimal? Persentase { get; set; }
        public bool? IsAktif { get; set; }
        public string? Keterangan { get; set; }
    }
}
