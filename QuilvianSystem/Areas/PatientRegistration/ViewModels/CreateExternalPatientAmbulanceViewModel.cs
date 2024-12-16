namespace QuilvianSystem.Areas.PatientRegistration.ViewModels
{
    public class CreateExternalPatientAmbulanceViewModel
    {
        public Guid ExternalPatientId { get; set; }
        public string KodePasien { get; set; }
        public string NomorRekamMedisBaru { get; set; }
        public string? NomorRekamMedisLama { get; set; }
        public string Title { get; set; }
        public string NamaPasien { get; set; }
        public string NomorIdentitasPasien { get; set; }
        public string TempatLahir { get; set; }
        public string TanggalLahir { get; set; }
        public string JenisKelamin { get; set; }
        public string AlamatLengkap { get; set; }
        public Guid? CountryId { get; set; }
        public Guid? ProvinceId { get; set; }
        public Guid? CityId { get; set; }
        public Guid? DistrictId { get; set; }
        public Guid? SubDistrictId { get; set; }
        public string? KodePos { get; set; }
        public string NomorTelepon { get; set; }
        public string EmailAktif { get; set; }
        public string Department { get; set; }
        public string Komponen { get; set; }
        public string DaerahTujuan { get; set; }
        public string KelebihanJarak { get; set; }
        public string KelebihanWaktu { get; set; }
        public string Paramedis { get; set; }
        public string AntarJemput { get; set; }
        public string Catatan { get; set; }
        public IFormFile? GenerateQrCode { get; set; }
    }
}
