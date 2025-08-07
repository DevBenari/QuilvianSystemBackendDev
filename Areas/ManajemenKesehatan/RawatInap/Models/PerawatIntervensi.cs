using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    public class PerawatIntervensi : UserActivity
    {
        [Key]
        public Guid IntervensiId { get; set; }
        public Guid? DiagnosaSDKIId { get; set; }
        public string? NamaIntervensi { get; set; }
        public string? TipeIntervensi { get; set; } // e.g., Hasil, Observasi, Terapeutik, Edukasi, Kolaborasi
        public string? Keterangan { get; set; }
    }
}
