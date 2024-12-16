namespace QuilvianSystem.Areas.HealthManagement.ViewModels
{
    public class DetailDoctorViewModel : CreateDoctorViewModel
    {
        public Guid DoctorId { get; set; }
        public string? DoctorPhotoPath { get; set; }
    }
}
