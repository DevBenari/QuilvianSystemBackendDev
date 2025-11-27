namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Gizi.ViewModels
{
    public class RecallViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public string? SikapPasienDiet { get; set; }
        public string? AnjuranDiet { get; set; }
        public DateTime? TglRecall { get; set; }
        public Guid? DietesienId { get; set; }
        public string? CatatanPerawat { get; set; }
        public List<RecallDetailViewModel>? Details { get; set; }
    }
}
