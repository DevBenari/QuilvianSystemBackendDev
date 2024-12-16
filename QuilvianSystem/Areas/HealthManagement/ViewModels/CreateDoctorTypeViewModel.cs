namespace QuilvianSystem.Areas.HealthManagement.ViewModels
{
    public class CreateDoctorTypeViewModel
    {
        public Guid DoctorTypeId { get; set; }
        public string KodeTipeDokter { get; set; }
        public string TipeDokter { get; set; }
        public string? Persentase { get; set; }
        public string Status { get; set; }
    }
}
