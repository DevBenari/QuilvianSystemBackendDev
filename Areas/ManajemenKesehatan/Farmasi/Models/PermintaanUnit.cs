using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models
{
    public class PermintaanUnit : UserActivity
    {
        [Key]
        public Guid PermintaanUnitId { get; set; }
        public Guid? UnitId { get; set; }
        public Guid? TujuanUnitId { get; set; }
        public string? JenisPermintaan { get; set; }
        public DateTime? TglPembuatanPermintaan { get; set; } 
        public string? StatusPermintaan { get; set; }
        public string? Keterangan { get; set; }

    }
}
