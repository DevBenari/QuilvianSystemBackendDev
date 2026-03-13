namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class PaketLayananDiskonVM
    {
        public Guid? PaketLayananId { get; set; }
        public Guid? PaketLayananAsuransiId { get; set; }
        public Guid? DiskonPercentageId { get; set; }
        public decimal? PotonganHargaMax { get; set; }
        public DateTime? PeriodeAwal { get; set; }
        public DateTime? PeriodeAkhir { get; set; }
        public string? Keterangan { get; set; }
    }
}
