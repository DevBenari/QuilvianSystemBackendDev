namespace QuilvianSystem.Areas.PatientRegistration.ViewModels
{
    public class CreateReferenceDetailViewModel
    {
        public Guid ReferenceDetailId { get; set; }
        public string KodeDetailRujukan { get; set; }
        public string NamaDetailRujukan { get; set; }
        public string NomorTelepon { get; set; }
        public string Alamat { get; set; }
        public Guid? ReferenceTypeId { get; set; }
    }
}
