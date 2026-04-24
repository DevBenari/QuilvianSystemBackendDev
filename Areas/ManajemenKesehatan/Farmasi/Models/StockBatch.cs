using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models
{
    public class StockBatch : UserActivity
    {
        [Key]
        public Guid? StockBatchId { get; set; }
        public string? KodeBatch {  get; set; }
        public Guid? ItemId { get; set; }
        public Guid? SupplierId { get; set; }
        public DateOnly? ExpiredDate { get; set; }
        public string? Keterangan {  get; set; }
    }
}
