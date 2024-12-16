namespace QuilvianSystem.Areas.PatientRegistration.ViewModels
{
    public class CreateExternalPatientRehabilitasiMedikViewModel
    {
        public Guid ExternalPatientId { get; set; }
        public string KodePasien { get; set; }
        public string NomorRekamMedisBaru { get; set; }
        public string? NomorRekamMedisLama { get; set; }
        public string TipePasien { get; set; }
        public Guid? InsuranceId { get; set; }
        public string? NomorPolis { get; set; }
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
        public string TipeRujukan { get; set; }
        public string DeskripsiRujukan { get; set; }
        public string SuratRujukan { get; set; }
        public string DiagnosaAwal { get; set; }
        public string DaftarTindakan { get; set; }
        public string DokterPemeriksa { get; set; }
        public IFormFile? GenerateQrCode { get; set; }
    }
}
