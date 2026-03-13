namespace QuilvianSystemBackendDev.ViewModels
{
    public class ShiftDenominasiViewModel
    {
        public string KodeShiftDenominasi { get; set; } = string.Empty;
        public Guid LayananId { get; set; }
        public Guid KasirId { get; set; }
        public string TipePerhitungan { get; set; } = string.Empty;
        public Guid DenominasiId { get; set; }
        public decimal LembarKoin { get; set; }
        public decimal TotalDenominasi { get; set; }
        public string? Keterangan { get; set; }
    }
}