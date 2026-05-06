using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    public class PaketLayananDetail : UserActivity
    {
        [Key]
        public Guid DetailPaketLayananId { get; set; }
        public Guid? DetailPaketId { get; set; }
        public Guid? LayananId { get; set; }
        public DateTime? TglPembuatan { get; set; }
        public string? Keterangan { get; set; }
    }
}
