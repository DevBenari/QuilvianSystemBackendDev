using QuilvianSystem.Areas.AccountingAndFinancial.Models;
using QuilvianSystem.Areas.Administration.Models;
using QuilvianSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystem.Areas.HealthManagement.Models
{
    [Table("HmnDoctor", Schema = "dbo")]
    public class Doctor : UserActivity //UserActivity Create on Repository
    {
        [Key]
        public Guid DoctorId { get; set; }
        public string KodeDokter { get; set; }
        public Guid? DoctorTypeId { get; set; }
        public string NamaLengkap { get; set; }
        public string? NamaMarga { get; set; }
        public string NomorKtpDokter { get; set; }
        public string? TempatLahir { get; set; }
        public string? TanggalLahir { get; set; }
        public string JenisKelamin { get; set; }
        public string Kewarganegaraan { get; set; }
        public Guid? LastEducationId { get; set; }
        public Guid? ReligionId { get; set; }
        public Guid? WorkingId { get; set; }
        public Guid? DoctorQueueTypeId { get; set; }
        public Guid? BankId { get; set; }
        public Guid? BankCabangId { get; set; }
        public string BankAtasNama { get; set; }
        public string BankNomorRekening { get; set; }
        public string? Npwp { get; set; }
        public string AlamatRumahLengkap { get; set; }
        public Guid? CountryId { get; set; }
        public Guid? ProvinceId { get; set; }
        public Guid? CityId { get; set; }
        public Guid? DistrictId { get; set; }
        public Guid? SubDistrictId { get; set; }
        public string? KodePos { get; set; }
        public string? NomorTelepon { get; set; }
        public string? NomorHandphone { get; set; }
        public string AlamatKantorLengkap { get; set; }
        public string? NomorTeleponKantor { get; set; }
        public string NomorIdDokter { get; set; }
        public Guid? DoctorTitleId { get; set; }
        public string? JenisKontrak { get; set; }
        public string? TanggalAwalKontrak { get; set; }
        public string? TanggalAkhirKontrak { get; set; }
        public string? TanggalKeluar { get; set; }
        public string? GuaranteeFee { get; set; }
        public string DokterMitra { get; set; }
        public string DokterSpesialis { get; set; }        
        public string? Foto { get; set; }        
        //public ICollection<MultipleDoctorDepartment> MultipleDoctorDepartment { get; set; }
        public List<MultipleDoctorDepartment> MultipleDoctorDepartments { get; set; } = new List<MultipleDoctorDepartment>();

        //Relationship
        [ForeignKey("DoctorTypeId")]
        public DoctorType? DoctorType { get; set; }
        [ForeignKey("LastEducationId")]
        public LastEducation? LastEducation { get; set; }
        [ForeignKey("ReligionId")]
        public Religion? Religion { get; set; }
        [ForeignKey("WorkingId")]
        public Working? Working { get; set; }
        [ForeignKey("DoctorQueueTypeId")]
        public DoctorQueueType? DoctorQueueType { get; set; }
        [ForeignKey("BankId")]
        public Bank? Bank { get; set; }
        [ForeignKey("BankCabangId")]
        public BankCabang? BankCabang { get; set; }
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
        [ForeignKey("DoctorTitleId")]
        public DoctorTitle? DoctorTitle { get; set; }
        //[ForeignKey("DepartmentId")]
        //public DoctorDepartment? DoctorDepartment { get; set; }
    }
}
