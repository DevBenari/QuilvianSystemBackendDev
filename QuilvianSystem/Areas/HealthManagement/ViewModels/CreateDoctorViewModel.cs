using QuilvianSystem.Areas.HealthManagement.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace QuilvianSystem.Areas.HealthManagement.ViewModels
{
    public class CreateDoctorViewModel
    {
        public Guid DoctorId { get; set; }
        public string KodeDokter { get; set; }
        public Guid? DoctorTypeId { get; set; }
        public string? DoctorType { get; set; }
        public string NamaLengkap { get; set; }
        public string? NamaMarga { get; set; }
        public string NomorKtpDokter { get; set; }
        public string? TempatLahir { get; set; }
        public string? TanggalLahir { get; set; }
        public string JenisKelamin { get; set; }
        public string Kewarganegaraan { get; set; }
        public Guid? LastEducationId { get; set; }
        public string? LastEducation { get; set; }
        public Guid? ReligionId { get; set; }
        public string? Religion { get; set; }
        public Guid? WorkingId { get; set; }
        public string? Working { get; set; }
        public Guid? DoctorQueueTypeId { get; set; }
        public string? DoctorQueueType { get; set; }
        public Guid? BankId { get; set; }
        public string? Bank { get; set; }
        public Guid? BankCabangId { get; set; }
        public string? BankCabang { get; set; }
        public string BankAtasNama { get; set; }
        public string BankNomorRekening { get; set; }
        public string? Npwp { get; set; }
        public string AlamatRumahLengkap { get; set; }
        public Guid? CountryId { get; set; }
        public string? Country { get; set; }
        public Guid? ProvinceId { get; set; }
        public string? Province { get; set; }
        public Guid? CityId { get; set; }
        public string? City { get; set; }
        public Guid? DistrictId { get; set; }
        public string? District { get; set; }
        public Guid? SubDistrictId { get; set; }
        public string? SubDistrict { get; set; }
        public string? KodePos { get; set; }
        public string? NomorTelepon { get; set; }
        public string? NomorHandphone { get; set; }
        public string AlamatKantorLengkap { get; set; }
        public string? NomorTeleponKantor { get; set; }
        public string NomorIdDokter { get; set; }
        public Guid? DoctorTitleId { get; set; }
        public string? DoctorTitle { get; set; }
        public string? JenisKontrak { get; set; }
        public string? TanggalAwalKontrak { get; set; }
        public string? TanggalAkhirKontrak { get; set; }
        public string? TanggalKeluar { get; set; }
        public string? GuaranteeFee { get; set; }
        public string DokterMitra { get; set; }
        public string DokterSpesialis { get; set; }
        //public string Department { get; set; }
        public List<SelectListItem>? DepartmentList { get; set; }
        public List<Guid> DepartmentId { get; set; }
        public IFormFile? Foto { get; set; }
    }
}
