namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class TindakanKunjunganViewModel
    {
        public Guid KunjunganId { get; set; }
        public Guid TindakanId { get; set; }
        public int? Quantity { get; set; }
        public Guid? DepartementId { get; set; }
        public Guid? DokterPemeriksaId { get; set; }
        public Guid? KelasId { get; set; }
        public string? TipeLayanan { get; set; }
        public DateTime? TanggalPemeriksaan { get; set; }
        //public decimal? Total { get; set; }
        public string? Disposition { get; set; }
        public string? Keterangan { get; set; }
        public Guid? DiskonId { get; set; }
        public bool? IsFoC { get; set; }
    }
}
