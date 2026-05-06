namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class TarifPatologiAnatomiViewModel
    {

        public Guid? LabPemeriksaanId { get; set; }
        public Guid? KelasId { get; set; }
        public decimal? TarifDokter { get; set; }
        public decimal? TarifRs { get; set; }
        public decimal? TarifJp { get; set; }
        public decimal? TarifBahp { get; set; }
        public decimal? TarifLain { get; set; }
        public decimal? TarifTotal { get; set; }
        public decimal? KSO { get; set; }
        public string? Keterangan { get; set; }
    }
}
