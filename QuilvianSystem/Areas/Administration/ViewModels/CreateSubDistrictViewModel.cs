using QuilvianSystem.Areas.Administration.Models;

namespace QuilvianSystem.Areas.Administration.ViewModels
{
    public class CreateSubDistrictViewModel
    {
        public Guid SubDistrictId { get; set; }
        public string KodeKelurahan { get; set; }
        public string NamaKelurahan { get; set; }
        public Guid? CountryId { get; set; }
        public Guid? ProvinceId { get; set; }
        public Guid? CityId { get; set; }
        public Guid? DistrictId { get; set; }       
    }
}
