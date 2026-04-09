namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class TarifKelasViewModel
    {
        public Guid? TindakanId { get; set; }
        public Guid? KelasId { get; set; }
        public Guid? PemeriksaanLabId { get; set; }
        public Guid? PeralatanId { get; set; }
        public Guid? DokterId { get; set; }
        public string? KategoriTindakan { get; set; }
        public string? KodeLayanan { get; set; }
        public string? NamaKelas { get; set; }
        public decimal? TarifDokter { get; set; }
        public decimal? TarifRs { get; set; }
        public decimal? TarifJp { get; set; }
        public decimal? TarifBahp { get; set; }
        public decimal? TarifLain { get; set; }
        public decimal? TarifTotal { get; set; }
        public decimal? KSO { get; set; }
    }
}
