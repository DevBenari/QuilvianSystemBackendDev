namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.ViewModels
{
    public class IGDObservasiDetailViewModel
    {
        public Guid? IGDObservasiDetailId { get; set; }
        public Guid? ObservasiIgdId { get; set; }
        public DateTime? TglObservasi { get; set; }
        public Guid? ObatId { get; set; }
        public string? GambaranEKG { get; set; }
        public string? DCShock { get; set; }
        public decimal? TekananDarahSystolic { get; set; }
        public decimal? TekananDarahDiastolic { get; set; }            
        public decimal? RR { get; set; }                  // Respiratory Rate
        public decimal? Suhu { get; set; }
        public decimal? SPO2 { get; set; }
        public decimal? Urine { get; set; }
        public decimal? Pendarahan { get; set; }
        public decimal? Muntah { get; set; }
        public string? Keterangan { get; set; }
    }
}
