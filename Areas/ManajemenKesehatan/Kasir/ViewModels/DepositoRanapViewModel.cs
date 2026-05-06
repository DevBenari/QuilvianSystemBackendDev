namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels
{
    public class DepositoRanapViewModel
    {
        public Guid? KunjunganId { get; set; }
        public string? NoKwitansi { get; set; }
        public decimal? NominalMasuk { get; set; }
        public decimal? NominalKeluar { get; set; }
        public string? StatusDeposit { get; set; }
        public string? Keterangan { get; set; }
    }
}
