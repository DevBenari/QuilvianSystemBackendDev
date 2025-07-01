namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels
{
    public class ObatReturnDetailViewModel
    {
        public Guid? ObatReturnId { get; set; }
        public Guid? ObatId { get; set; }
        public string? NamaObat { get; set; }
        public int? Qty { get; set; }
        public string? NoBatch { get; set; }
        public bool? IsMasihTersegel { get; set; }
        public bool? IsObatUtuh { get; set; }
        public string? Keterangan { get; set; }
    }
}
