namespace QuilvianSystem.Areas.HealthManagement.ViewModels
{
    public class CreateDoctorTitleViewModel
    {
        public Guid DoctorTitleId { get; set; }
        public string KodeGelar { get; set; }
        public string NamaGelar { get; set; }
        public string? Deskripsi { get; set; }
        public string LapRL1 { get; set; }
        public string LapRL2 { get; set; }
        public string Status { get; set; }
    }
}
