using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class KelurahanViewModel
    {
        public string KelurahanCode { get; set; }
        public string KelurahanName { get; set; }

        // Foreign key to KecamatanId (Guid)
        public Guid KecamatanId { get; set; }  // Changed to match KecamatanId type (Guid)
        //[ForeignKey("KecamatanId")]
        //public Kecamatan Kecamatan { get; set; }  // Relationship with Kecamatan
    }
}
