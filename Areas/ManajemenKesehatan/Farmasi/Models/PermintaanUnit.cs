using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Areas.HRD.MasterData.Models;
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

        // navigation ke tabel InstalasiUnit
        public InstalasiUnit? Unit { get; set; }
        public InstalasiUnit? TujuanUnit { get; set; }

        // header -> detail
        public ICollection<DetailPermintaanUnit> DetailPermintaanUnits { get; set; } = new List<DetailPermintaanUnit>();

    }
}
