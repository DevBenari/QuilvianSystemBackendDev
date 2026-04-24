namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Alkes.ViewModels
{
    public class AlatPemakaianDetailViewModel
    {
        //public Guid? DetailPemakaianAlatId { get; set; } // <== untuk update baris existing
        public Guid? PeralatanId { get; set; }
        public Guid? KelasId { get; set; }
        public int? QtyPemakaian { get; set; }
        //public decimal? HargaPeralatan { get; set; }
        //public decimal? TotalPemakaianAlat { get; set; }
        public string? Keterangan { get; set; }
    }
}
