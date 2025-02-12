using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class KecamatanViewModel
    {
        public string KecamatanCode { get; set; }
        public string KecamatanName { get; set; }

        // Foreign key to KabupatenId (Guid)
        public Guid KabupatenId { get; set; }  // Changed to match KabupatenId type (Guid)
        //[ForeignKey("KabupatenId")]
        //public Kabupaten Kabupaten { get; set; }  // Relationship with Kabupaten
    }
}
