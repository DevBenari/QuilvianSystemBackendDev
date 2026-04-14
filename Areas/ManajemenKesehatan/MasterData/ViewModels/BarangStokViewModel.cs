namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class BarangStokViewModel
    {
        public Guid? BarangId { get; set; }
        public Guid? LokasiPenyimpananId { get; set; }
        public decimal? QtyStokBarang { get; set; }
        public string? Keterangan { get; set; }
    }
}
