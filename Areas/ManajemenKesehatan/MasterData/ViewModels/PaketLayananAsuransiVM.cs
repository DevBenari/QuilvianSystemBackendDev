namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class PaketLayananAsuransiVM
    {
        public Guid? PaketLayananId { get; set; }
        public Guid? AsuransiId { get; set; }
        public Guid? CorporateId { get; set; }
        public DateTime? TglPembuatan { get; set; }
        public string? Keterangan { get; set; }
    }
}
