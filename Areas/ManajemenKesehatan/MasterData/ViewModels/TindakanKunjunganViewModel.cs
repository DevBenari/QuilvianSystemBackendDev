namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class TindakanKunjunganViewModel
    {
        public Guid KunjunganId { get; set; }
        public Guid TindakanId { get; set; }
        public int? Quantity { get; set; }
        public Guid? RanapId { get; set; }
        public Guid? DepartementId { get; set; }
        public Guid? DokterPemeriksaId { get; set; }
        public Guid? KelasId { get; set; }
        //public decimal? Total { get; set; }
        public string? Disposition { get; set; }
        public Guid? DiskonId { get; set; }
    }
}
