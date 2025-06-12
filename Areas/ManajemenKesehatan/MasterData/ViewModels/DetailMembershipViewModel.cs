namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class DetailMembershipViewModel
    {
        public Guid? MembershipId { get; set; }
        public Guid? BenefitId { get; set; }
        public string? Keterangan { get; set; }
        public bool? IsActive { get; set; } = true;
    }
}
