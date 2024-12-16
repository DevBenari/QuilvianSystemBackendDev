using QuilvianSystem.Areas.Administration.Models;

namespace QuilvianSystem.Areas.Administration.ViewModels
{
    public class CreateProvinceViewModel
    {
        public Guid ProvinceId { get; set; }
        public string KodeProvinsi { get; set; }
        public string NamaProvinsi { get; set; }
        public Guid? CountryId { get; set; }
    }
}
