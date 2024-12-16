namespace QuilvianSystem.Areas.HealthManagement.ViewModels
{
    public class CreateDoctorDepartmentLocationViewModel
    {
        public Guid LocationId { get; set; }
        public string KodeLokasi { get; set; }
        public string NamaLokasi { get; set; }
        public string? Keterangan { get; set; }
    }
}
