namespace QuilvianSystemBackendDev.Areas.Keuangan.Kasir.ViewModels
{
    public class DiskonViewModel
    {
        public string? NamaDiskon { get; set; }
        public DateOnly? TglBerlaku { get; set; }
        public DateOnly? TglBerakhir { get; set; }
        public bool? IsAsuransi { get; set; }
        public Guid? AsuransiId { get; set; }
        public decimal? PersenDiskon { get; set; }
        public decimal? NominalDiskon { get; set; }
        public string? Keterangan { get; set; }
    }
}
