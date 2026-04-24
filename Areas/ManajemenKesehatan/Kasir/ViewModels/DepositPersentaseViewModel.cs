namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels
{
    public class DepositPersentaseViewModel
    {
        public decimal? LimitPersentase { get; set; }
        public DateTime? AwalPeriode { get; set; }
        public DateTime? AkhirPeriode { get; set; }
        public string? Keterangan { get; set; }
    }
}
