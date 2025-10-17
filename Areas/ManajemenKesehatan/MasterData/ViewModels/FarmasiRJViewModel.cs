namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class FarmasiRJViewModel
    {
        public Guid? ObatId { get; set; }
        public Guid? KonversiSatuanId { get; set; }
        public decimal? QtySatuan { get; set; }
        public decimal? QtyKonversi { get; set; }
        public string? BatchNumber { get; set; }
        public string? RackLocation { get; set; }
        public DateOnly? TanggalKadaluarsa { get; set; }
    }
}
