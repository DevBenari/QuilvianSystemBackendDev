using QuilvianSystem.Areas.Administration.Models;
using QuilvianSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystem.Areas.PatientRegistration.Models
{
    [Table("PtrExternalPatientLaboratorium", Schema = "dbo")]
    public class ExternalPatientLaboratorium : UserActivity //UserActivity Create on Repository
    {
        [Key]
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
        public Guid? PromoId { get; set; }
        public string TipePemeriksaan { get; set; }
        public string SuratRujukan { get; set; }
        public string DiagnosaAwal { get; set; }
        public string TanggalSampling { get; set; }
        public string DetailTindakan { get; set; }
        public string DokterPemeriksa { get; set; }
        public string? Pemeriksaan { get; set; }
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
        [ForeignKey("PromoId")]
        public Promo? Promo { get; set; }
    }
}
