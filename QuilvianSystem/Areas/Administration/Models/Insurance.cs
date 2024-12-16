
using QuilvianSystem.Areas.PatientRegistration.Models;
using QuilvianSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystem.Areas.Administration.Models
{ 

    [Table("AdmInsurance", Schema = "dbo")]
    public class Insurance : UserActivity //UserActivity Create on Repository
    {
        [Key]
        public Guid InsuranceId { get; set; }
        public string KodeAsuransi { get; set; }
        public string MulaiKerjasama { get; set; }
        public string AkhirKerjasama { get; set; }
        public string? KodePerusahaan { get; set; }
        public string TipePerusahaan { get; set; }
        public string NamaPerusahaan { get; set; }
        public string TarifGroupPerusahaan { get; set; }
        public string Email { get; set; }
        public string AkunBankKartuKredit { get; set; }        
        public string? KomisiKartuKredit { get; set; }
        public string? Diskon { get; set; }
        public string TermasukAsuransi { get; set; }
        public string TermasukKaryawanRS { get; set; }
        //public string PerusahaanAsuransi { get; set; }
        public string Direktur { get; set; }
        public string NamaKontak { get; set; }
        public string? Jabatan { get; set; }
        public string? Bagian { get; set; }
        public string Alamat { get; set; }
        public string AlamatTagihan { get; set; }
        public Guid? CountryId { get; set; }
        public Guid? ProvinceId { get; set; }
        public Guid? CityId { get; set; }
        public Guid? DistrictId { get; set; }
        public Guid? SubDistrictId { get; set; }
        public string? KodePos { get; set; }
        public string NomorTelepon { get; set; }
        public string? NomorFax { get; set; }
        public string Status { get; set; }
        public string JenisKerjasama { get; set; }
        public string JenisKontrak { get; set; }
        public string JatuhTempo { get; set; }
        public string KriteriaPembayaran { get; set; }
        public string MenjaminPasienOTC { get; set; }
        public string AkunBankAtasNama { get; set; }
        public string NamaBank { get; set; }
        public string NamaCabang { get; set; }
        public string NomorRekeningBank { get; set; }
        public string? Pinalti { get; set; }
        public string? Keterangan { get; set; }

        public ICollection<NewPatient> NewPatient { get; set; }

        //Relationship       
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
    }
}
