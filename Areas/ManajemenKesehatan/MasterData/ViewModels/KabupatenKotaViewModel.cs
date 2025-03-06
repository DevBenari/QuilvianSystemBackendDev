using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class KabupatenKotaViewModel
    {
        public string NamaKabupatenKota { get; set; }
        public Guid ProvinsiId { get; set; }
    }
}
