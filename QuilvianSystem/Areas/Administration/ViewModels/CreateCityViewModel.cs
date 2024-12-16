using QuilvianSystem.Areas.Administration.Models;

namespace QuilvianSystem.Areas.Administration.ViewModels
{
    public class CreateCityViewModel
    {
        public Guid CityId { get; set; }
        public string KodeKota { get; set; }
        public string NamaKota { get; set; }
        public Guid? CountryId { get; set; }
        public Guid? ProvinceId { get; set; }
    }
}
