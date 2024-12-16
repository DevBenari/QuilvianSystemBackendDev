namespace QuilvianSystem.Areas.PatientRegistration.ViewModels
{
    public class CreateReferenceTypeViewModel
    {
        public Guid ReferenceTypeId { get; set; }
        public string KodeTipeRujukan { get; set; }
        public string NamaTipeRujukan { get; set; }
        public Guid? ReferenceId { get; set; }
    }
}
