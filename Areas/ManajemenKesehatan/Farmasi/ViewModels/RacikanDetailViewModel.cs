using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels
{
    public class RacikanDetailViewModel
    {
        public Guid? RacikanId { get; set; }
        public Guid? ObatId { get; set; }
        public decimal? KomposisiDosis { get; set; }
    }
}
