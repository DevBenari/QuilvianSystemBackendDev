namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class BarangHargaViewModel
    {
        public Guid? BarangId { get; set; }
        public decimal? HteHargaBarang { get; set; }
        public decimal? HneHargaBarang { get; set; }
        public DateTime? TglBerlaku { get; set; }
        public DateTime? TglBerakhir { get; set; }
        public string? Keterangan { get; set; }
    }
}
