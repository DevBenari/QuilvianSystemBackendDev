using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class ResepViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? AsuransiId { get; set; }
        public List<DetailResepViewModel>? DaftarObat { get; set; }
    }
}
