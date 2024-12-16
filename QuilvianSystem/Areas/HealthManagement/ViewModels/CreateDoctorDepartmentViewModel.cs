namespace QuilvianSystem.Areas.HealthManagement.ViewModels
{
    public class CreateDoctorDepartmentViewModel
    {
        public Guid DepartmentId { get; set; }
        public string KodeDepartemen { get; set; }
        public string NamaDepartemen { get; set; }
        public Guid? LocationId { get; set; }
        public string? Telepon { get; set; }
        public string MulaiJamKerja { get; set; }
        public string SelesaiJamKerja { get; set; }
        public string? Keterangan { get; set; }
    }
}
