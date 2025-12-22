namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Alkes.ViewModels
{
    public class AlatPemakaianDetailViewModel
    {
        public Guid? PeralatanId { get; set; }
        public int? QtyPemakaian { get; set; }
        public decimal? HargaPeralatan { get; set; }
        public decimal? TotalPemakaianAlat { get; set; }
        public decimal? Keterangan { get; set; }
    }
}
