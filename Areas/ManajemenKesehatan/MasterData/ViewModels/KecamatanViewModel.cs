using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class KecamatanViewModel
    {
        public string NamaKecamatan { get; set; }
        public Guid KabupatenKotaId { get; set; }
    }
}
