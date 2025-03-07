using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class KelurahanViewModel
    {
        public string NamaKelurahan { get; set; }
        public Guid KecamatanId { get; set; }
    }
}
