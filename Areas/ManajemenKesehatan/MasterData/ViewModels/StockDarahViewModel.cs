namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class StockDarahViewModel
    {
        public Guid GolonganDarahId { get; set; }
        public Guid TipeKomponenId { get; set; }
        public string? Rhesus { get; set; }
        public string? Golongan { get; set; }
        public decimal? Wacc { get; set; }
        public decimal? JumlahKantong { get; set; }
        public decimal? Amount { get; set; }
        public decimal? JumlahExpired { get; set; }
        public DateTime? TglExpired { get; set; }
        public decimal? SisaStock { get; set; }
        public decimal? MinStock { get; set; }
        public string? StatusStock { get; set; }
        public string? Keterangan { get; set; }
    }

}
