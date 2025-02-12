using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class ProvinsiViewModel
    {
        public string ProvinsiCode { get; set; }
        public string ProvinsiName { get; set; }

        public Guid NegaraId { get; set; }

        // Relationship with Kabupaten
        //public ICollection<Kabupaten> Kabupaten { get; set; }
    }
}
