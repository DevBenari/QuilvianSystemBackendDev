namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels
{
    public class DiskonTagihanViewModel
    {
        public Guid? DiskonId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public string? NamaDiskon { get; set; }
        public decimal? ValueDiskon { get; set; }
        public string? Keterangan { get; set; }
    }
}
