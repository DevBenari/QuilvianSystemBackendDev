namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class PasienBenefitOverideViewModel
    {
        public Guid? PasienId { get; set; }
        public Guid? EntityId { get; set; }
        public string? EntityType { get; set; }
        public string? Sumber { get; set; } // Contoh: "Membership", "PendaftaranPasienBaru", dll.
        public decimal? BiayaTambahan { get; set; }
        public bool? Diskon { get; set; }
        public bool? IsAktif { get; set; } = true;
        public bool? IsGratis { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
    }
}
