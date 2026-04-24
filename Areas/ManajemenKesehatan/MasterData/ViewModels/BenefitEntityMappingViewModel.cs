namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class BenefitEntityMappingViewModel
    {
        public Guid? BenefitId { get; set; }
        public Guid? EntityId { get; set; }
        public string? EntityType { get; set; } // Contoh: "Membership", "PendaftaranPasienBaru", dll.
        public decimal? Kuota { get; set; }
        public decimal? Diskon { get; set; }
        public bool? IsGratis { get; set; }
    }
}
