namespace QuilvianSystem.Areas.HealthManagement.ViewModels
{
    public class CreateBloodTypeViewModel
    {
        public Guid BloodTypeId { get; set; }
        public string KodeGolonganDarah { get; set; }
        public string NamaGolonganDarah { get; set; }
        public string? Keterangan { get; set; }
    }
}
