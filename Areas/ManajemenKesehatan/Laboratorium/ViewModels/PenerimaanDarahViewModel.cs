using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels
{
    public class PenerimaanDarahViewModel
    {

        public DateTime? TglPenerimaan { get; set; }
        public DateTime? TglFaktur { get; set; }
        public string? NoFaktur { get; set; }
        public string? NoPO { get; set; }
        public Guid? SupplierId { get; set; }
        public Guid? PenerimaId { get; set; }
        public Guid? DarahDetailId { get; set; }
        public decimal? JumlahKantong { get; set; }
        public string? Keterangan { get; set; }

        // list stock darah
        public List<StockDarahViewModel> StockDarah { get; set; }
       
        // list stock batch
        public List<StockBatchViewModel> StockBatch { get; set; }
    }

}
