using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.Finance.AP.Models
{
    [Table("Fin_ReceiveOrderItem", Schema = "public")]
    public class ReceiveOrderItem : UserActivity
    {
        [Key]
        public Guid ReceiveOrderItemId { get; set; }

        public Guid? ReceiveOrderId { get; set; }

        public Guid? ProductId { get; set; }

        [MaxLength(100)]
        public string? Barcode { get; set; }

        [MaxLength(250)]
        public string? ProductName { get; set; }

        [MaxLength(100)]
        public string? Measure { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? QtyOrder { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? QtyReceive { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? StampDuty { get; set; }

        public DateTime? ExpiredDate { get; set; }

        [MaxLength(100)]
        public string? BatchNumber { get; set; }

        // tambahan


        [Column(TypeName = "numeric(18,2)")]
        public decimal HargaSatuan { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal HargaTotal { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal DiskonProduk { get; set; }


        [MaxLength(500)]
        public string? Keterangan { get; set; }

    }
}
