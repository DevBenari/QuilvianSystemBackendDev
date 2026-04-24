using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models
{
    public class StockKartu : UserActivity
    {
        [Key]
        public Guid KartuStockId { get; set; }
        public Guid? ObatId { get; set; }
        public Guid? BatchId { get; set; }
        public Guid? UnitAsalId { get; set; }
        public Guid? UnitTujuanId { get; set; }
        public Guid? SatuanId { get; set; }
        public Guid? KonversiSatuanId { get; set; }
        public decimal? Qty { get; set; }
        public decimal? QtyKonversi { get; set; }
        public string? JenisTransaksi { get; set; }
        public string? IO {  get; set; }
        public string? Keterangan {  get; set; }
    }
}
