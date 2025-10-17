using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    public class GudangUnit : UserActivity
    {
        [Key]
        public Guid GudangUnitId { get; set; } 
        public Guid? GudangId { get; set; }
        public Guid? ObatId { get; set; }
        public decimal? StockGudangUnit { get; set; } 
        public decimal? MinStockGudangUnit { get; set; }
        public decimal? MaxStockGudangUnit { get; set; }
        public decimal? StockPenyanggaGudangUnit { get; set; }
        public string? Keterangan { get; set; }
    }
}
