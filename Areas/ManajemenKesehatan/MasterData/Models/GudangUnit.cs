using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Areas.HRD.MasterData.Models;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    public class GudangUnit : UserActivity
    {
        [Key]
        public Guid GudangUnitId { get; set; } 
        public Guid? GudangId { get; set; }
        public Guid? InstalasiUnitId { get; set; }
        public string? NamaGudangUnit { get; set; }
        public string? KodeGudangUnit { get; set; }
        public string? Keterangan { get; set; }

        //Relation        
        public Gudang? Gudang { get; set; }
        public InstalasiUnit? InstalasiUnit { get; set; }
        public ICollection<ObatUnit>? ObatUnits { get; set; }
    }
}
