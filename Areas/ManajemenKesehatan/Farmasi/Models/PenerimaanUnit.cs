using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Areas.HRD.MasterData.Models;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models
{
    public class PenerimaanUnit : UserActivity
    {
        [Key]
        public Guid PenerimaanUnitId { get; set; }
        public Guid? UnitId { get; set; }
        //public string? JenisPermintaan { get; set; }
        public DateTime? TglPenerimaan { get; set; }
        public string? StatusPenerimaan { get; set; }
        public string? Keterangan { get; set; }

        // navigation ke tabel InstalasiUnit
        public InstalasiUnit? Unit { get; set; }

        // header -> detail
        public ICollection<DetailPenerimaanUnit> DetailPenerimaanUnits { get; set; } = new List<DetailPenerimaanUnit>();
    }
}
