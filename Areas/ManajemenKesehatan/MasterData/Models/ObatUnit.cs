using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Areas.HRD.MasterData.Models;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstObatUnit", Schema = "public")]
    public class ObatUnit : UserActivity
    {
        [Key]
        public Guid ObatUnitId { get; set; }
        public Guid? ObatId { get; set; }
        public Guid? InstalasiUnitId { get; set; }
        public decimal? Qty { get; set; }
        public decimal? QtyAmbil {  get; set; }
        public decimal? QtyTersedia { get; set; }
        
        // navigation
        public Obat? Obat { get; set; }
        public InstalasiUnit? InstalasiUnit { get; set; }
    }
}
