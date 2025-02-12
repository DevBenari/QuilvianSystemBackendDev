using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class KabupatenViewModel
    {
        public string KabupatenKotaCode { get; set; }
        public string KabupatenKotaName { get; set; }

        // Foreign key to ProvinsiId (Guid)
        public Guid ProvinsiId { get; set; }
        //[ForeignKey("ProvinsiId")]
        //public Provinsi Provinsi { get; set; } // Relationship with Provinsi
    }
}
