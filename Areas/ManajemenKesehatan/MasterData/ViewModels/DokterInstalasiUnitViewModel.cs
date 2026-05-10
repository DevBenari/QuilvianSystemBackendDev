using QuilvianSystemBackendDev.Areas.HRD.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class DokterInstalasiUnitViewModel
    {
        public Guid DokterId { get; set; }

        public Guid InstalasiUnitId { get; set; }

    }
}
