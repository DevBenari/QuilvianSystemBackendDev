namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class KonversiSatuanViewModel
    {
        public Guid? ObatId { get; set; }
        public Guid? SatuanId { get; set; }
        public string? NamaSatuan { get; set; }
        public string? TipeKonversi { get; set; } // e.g., "Pcs", "Box", "Botol"
        public decimal? NilaiKonversi { get; set; }
    }
}
