namespace QuilvianSystemBackendDev.ViewModels
{
    public class MasterDenominasiViewModel
    {
        public string KodeDenominasi { get; set; } = string.Empty;
        public decimal MataUang { get; set; }
        public decimal NominalPecahan { get; set; }
        public string? Keterangan { get; set; }
    }
}