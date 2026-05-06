using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.Models
{
    public class PindahRuangan : UserActivity
    {
        [Key]
        public Guid PindahRuanganId { get; set; }
        public Guid? UnitId { get; set; }
        public Guid? KamarId { get; set; }
        public DateTime? TglAwal {  get; set; }
        public DateTime? TglAkhir {  get; set; }
        public string? Keterangan {  get; set; }
    }
}
