
using QuilvianSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystem.Areas.Administration.Models
{
    [Table("AdmCountry", Schema = "dbo")]
    public class Country : UserActivity
    {
        [Key]
        public Guid CountryId { get; set; }
        public string KodeNegara { get; set; }
        public string NamaNegara { get; set; }
        public ICollection<Province> Province { get; set; }       
    }

    [Table("AdmProvince", Schema = "dbo")]
    public class Province : UserActivity
    {
        [Key]
        public Guid ProvinceId { get; set; }
        public string KodeProvinsi { get; set; }
        public string NamaProvinsi { get; set; }
        public Guid? CountryId { get; set; }        
        public ICollection<City> City { get; set; }

        //Relationship
        [ForeignKey("CountryId")]
        public Country? Country { get; set; }
    }

    [Table("AdmCity", Schema = "dbo")]
    public class City : UserActivity
    {
        [Key]
        public Guid CityId { get; set; }
        public string KodeKota { get; set; }
        public string NamaKota { get; set; }
        public Guid? CountryId { get; set; }
        public Guid? ProvinceId { get; set; }
        public ICollection<District> District { get; set; }

        //Relationship
        [ForeignKey("CountryId")]
        public Country? Country { get; set; }
        [ForeignKey("ProvinceId")]
        public Province? Province { get; set; }
    }

    [Table("AdmDistrict", Schema = "dbo")]
    public class District : UserActivity
    {
        [Key]
        public Guid DistrictId { get; set; }
        public string KodeKecamatan { get; set; }
        public string NamaKecamatan { get; set; }
        public Guid? CountryId { get; set; }
        public Guid? ProvinceId { get; set; }
        public Guid? CityId { get; set; }       
        public ICollection<SubDistrict> SubDistrict { get; set; }

        //Relationship
        [ForeignKey("CountryId")]
        public Country? Country { get; set; }
        [ForeignKey("ProvinceId")]
        public Province? Province { get; set; }
        [ForeignKey("CityId")]
        public City? City { get; set; }
    }

    [Table("AdmSubDistrict", Schema = "dbo")]
    public class SubDistrict : UserActivity
    {
        [Key]
        public Guid SubDistrictId { get; set; }
        public string KodeKelurahan { get; set; }
        public string NamaKelurahan { get; set; }
        public Guid? CountryId { get; set; }
        public Guid? ProvinceId { get; set; }
        public Guid? CityId { get; set; }
        public Guid? DistrictId { get; set; }

        //Relationship        
        [ForeignKey("CountryId")]
        public Country? Country { get; set; }
        [ForeignKey("ProvinceId")]
        public Province? Province { get; set; }
        [ForeignKey("CityId")]
        public City? City { get; set; }
        [ForeignKey("DistrictId")]
        public District? District { get; set; }
    }    
}
