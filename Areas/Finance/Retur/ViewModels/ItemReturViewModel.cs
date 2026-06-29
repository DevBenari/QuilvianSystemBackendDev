using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Finance.Retur.ViewModels
{
    public class ItemReturViewModel
    {
        [Required]
        public Guid ProdukId { get; set; }

        [Required]
        public Guid HeaderReturId { get; set; }

        public string? NoBatch { get; set; }

        public string? NoFakturInvoice { get; set; }

        public string? NoPO { get; set; }

        [Required]
        public Guid POId { get; set; }

        public decimal QtyDiterima { get; set; }

        public decimal QtyTelahDiretur { get; set; }

        [Required]
        public Guid ReceiveOrderId { get; set; }

        public decimal QtyRetur { get; set; }

        public string? Satuan { get; set; }

        public decimal HargaSatuan { get; set; }

        public DateTime TglPenerimaanPO { get; set; }

        public DateTime? TglTukarFaktur { get; set; }

        // tambahan

        [Column(TypeName = "numeric(18,2)")]
        public decimal? HargaTotal { get; set; }

        //end
        public string? Keterangan { get; set; }
    }
}