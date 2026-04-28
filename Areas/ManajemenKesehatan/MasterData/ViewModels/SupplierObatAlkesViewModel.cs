namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class SupplierObatAlkesViewModel
    {
        public Guid? ObatAlkesId { get; set; }
        public Guid? SupplierId { get; set; }
        public decimal? MinOrder { get; set; }
        public decimal? HargaBeli { get; set; }
        public bool? IsUtama { get; set; }
        public string? Keterangan { get; set; }

    }
}
