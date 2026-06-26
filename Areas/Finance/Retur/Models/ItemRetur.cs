using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Finance.Retur.Models
{
    [Table("Fin_ItemRetur", Schema = "public")]
    public class ItemRetur : UserActivity
    {
        [Key]
        public Guid ItemReturId { get; set; } = Guid.NewGuid();

        public Guid ProdukId { get; set; }

        public Guid HeaderReturId { get; set; }

        [MaxLength(50)]
        public string? StatusRetur { get; set; }

        public bool IsTerkonfirmasi { get; set; }

        public DateTime TglRetur { get; set; }

        [MaxLength(100)]
        public string? NoBatch { get; set; }

        [MaxLength(100)]
        public string? NoFakturInvoice { get; set; }

        [MaxLength(100)]
        public string? NoPO { get; set; }

        public Guid POId { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal QtyDiterima { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal QtyTelahDiretur { get; set; }

        public Guid ReceiveOrderId { get; set; }

        [NotMapped]
        public string? ReceiveNumber { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal QtyRetur { get; set; }

        [MaxLength(50)]
        public string? Satuan { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal HargaSatuan { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal SubtotalHarga { get; set; }

        public DateTime TglPenerimaanPO { get; set; }

        public DateTime? TglTukarFaktur { get; set; }

        public string? Keterangan { get; set; }
    }
}