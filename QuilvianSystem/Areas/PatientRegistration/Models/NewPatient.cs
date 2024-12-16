using QuilvianSystem.Areas.Administration.Models;
using QuilvianSystem.Areas.HealthManagement.Models;
using QuilvianSystem.Models;
using QuilvianSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystem.Areas.PatientRegistration.Models
{
    [Table("PtrNewPatient", Schema = "dbo")]
    public class NewPatient : UserActivity
    {
        [Key]
        public Guid PatientRegistrationId { get; set; }
        public string KodePasien { get; set; }
        public string NomorRekamMedisBaru { get; set; }
        public string? NomorRekamMedisLama { get; set; }
        public string TipePasien { get; set; }
        public Guid? InsuranceId { get; set; }
        public string? NomorPolis { get; set; }
        public string NamaLengkapPasien { get; set; }
        public string Title { get; set; }
        public string NomorIdentitasPasien { get; set; }
        public string TempatLahir { get; set; }
        public string TanggalLahir { get; set; }
        public string JenisKelamin { get; set; }
        public string PasienPrioritas { get; set; }
        public string StatusPasien { get; set; }
        public Guid? ReligionId { get; set; }
        public string Kewarganegaraan { get; set; }
        public Guid? LastEducationId { get; set; }
        public string AlamatLengkap { get; set; }
        public Guid? CountryId { get; set; }
        public Guid? ProvinceId { get; set; }
        public Guid? CityId { get; set; }
        public Guid? DistrictId { get; set; }
        public Guid? SubDistrictId { get; set; }
        public string? KodePos { get; set; }
        public string NomorTelepon1 { get; set; }
        public string? NomorTelepon2 { get; set; }
        public string Email { get; set; }
        public string? Pekerjaan{ get; set; }
        public string? NamaKantor { get; set; }
        public string? AlamatKantor { get; set; }
        public string? NomorTeleponKantor { get; set; }
        public Guid? BloodTypeId { get; set; }
        public string Alergi { get; set; }
        public string NamaKeluargaTerdekat { get; set; }
        public string HubunganKeluarga { get; set; }
        public string KaryawanRumahSakit { get; set; }
        public string AlamatKeluargaPasien { get; set; }
        public string NomorTeleponKeluargaPasien { get; set; }
        public string NamaAyahPasien { get; set; }
        public string? PekerjaanAyahPasien { get; set; }
        public string NamaIbuPasien { get; set; }
        public string? PekerjaanIbuPasien { get; set; }
        public string? NamaSutriPasien { get; set; }
        public string? PekerjaanSutriPasien { get; set; }
        public string? NomorIdentitasSutriPasien { get; set; }
        public string GenerateQrCode { get; set; }

        //Relationship
        [ForeignKey("InsuranceId")]
        public Insurance? Insurance { get; set; }
        [ForeignKey("CountryId")]
        public Country? Country { get; set; }
        [ForeignKey("ProvinceId")]
        public Province? Province { get; set; }
        [ForeignKey("CityId")]
        public City? City { get; set; }
        [ForeignKey("DistrictId")]
        public District? District { get; set; }
        [ForeignKey("SubDistrictId")]
        public SubDistrict? SubDistrict { get; set; }
        [ForeignKey("BloodTypeId")]
        public BloodType? BloodType { get; set; }
    }
}
