namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class PasienBenefitAsignViewModel
    {
        public Guid? PasienId { get; set; }
        public Guid? BenefitId { get; set; }
        public bool? IsActive { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
    }
}
