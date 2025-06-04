namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class VitalSignViewModel
    {
        public Guid? KunjunganId { get; set; }
        public decimal? Suhu { get; set; }
        public int? HR { get; set; }
        public int? RR { get; set; }
        public int? TekananDarahSystolic { get; set; }
        public int? TekananDarahDiastolic { get; set; }
        public decimal? SaturasiOksigen { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public decimal? BMI { get; set; }
        public decimal? LingkarKepalaBayi { get; set; }
    }
}
