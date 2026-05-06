using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    public class StockDarah
    {
        [Key]
        public Guid StockDarahId { get; set; }
        public Guid? DarahDetailId { get; set; }
        public Guid? TipeKomponenId { get; set; }
        public string? Rhesus { get; set; }
        public string? Golongan { get; set; }
        public decimal? Wacc { get; set; }
        public decimal? JumlahKantong { get; set; }
        public decimal? Amount { get; set; }
        public decimal? JumlahExpired { get; set; }
        public DateTime? TglExpired { get; set; }
        public decimal? SisaStock { get; set; }
        public decimal? MinStock { get; set; }
        public string? StatusStock { get; set; }
        public string? Keterangan { get; set; }
        public Guid? SupplierId { get; set; }

        // Common audit fields
        public Guid? CreateBy { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? DeleteBy { get; set; }
        public DateTime? DeleteDateTime { get; set; }
        public bool IsDelete { get; set; } = false;
    }

}
