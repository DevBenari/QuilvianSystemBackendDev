namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels
{
    public class CatatanESOViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? CttPemberianObatId { get; set; }
        public Guid? ObatId { get; set; }
        public Guid? RacikanId { get; set; }
        public bool? IsTandaiObat { get; set; }
        public string? TglTerjadi { get; set; }
        public string? ManifestasiESO { get; set; }
        public string? TglKesudahan { get; set; }
        public Guid? PerawatUserActiveId { get; set; }
        //public Guid? TTDid { get; set; }
        public string? Keterangan { get; set; }

        // Tambahan untuk upload TTD
        public IFormFile? TTDFile { get; set; }
    }
}
