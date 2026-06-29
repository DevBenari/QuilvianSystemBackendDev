using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Finance.AP.Models
{
    [Table("Fin_RekapAP", Schema = "public")]
    public class RekapAP : UserActivity
    {
        [Key]
        public Guid RekapAPId { get; set; } = Guid.NewGuid();

        public Guid? SupplierId { get; set; }

        // GET saja dari master supplier by SupplierId
        [NotMapped]
        public string? NamaSupplier { get; set; }

        // Kalkulasi HargaTotalPO dari ReceiveOrder berdasarkan SupplierId
        [NotMapped]
        public decimal RekapPenerimaan { get; set; }

        // Kalkulasi TotalInvoiceDetail / detail tukar faktur berdasarkan SupplierId
        [NotMapped]
        public decimal RekapDiakui { get; set; }

        // Kalkulasi NominalPPN dari ReceiveOrder berdasarkan SupplierId
        [NotMapped]
        public decimal RekapPPN { get; set; }

        // Manual / bisa diinput jika dibutuhkan
        [Column(TypeName = "numeric")]
        public decimal? RekapVariasiHarga { get; set; }

        // Kalkulasi TotalDiskon dari ReceiveOrder berdasarkan SupplierId
        [NotMapped]
        public decimal RekapDiskon { get; set; }

        // Kalkulasi HargaTotal / SubtotalHarga dari ItemRetur berdasarkan SupplierId
        [NotMapped]
        public decimal RekapRetur { get; set; }

        // Manual / bisa diinput jika dibutuhkan
        [Column(TypeName = "numeric")]
        public decimal? RekapOther { get; set; }

        // Rumus BE:
        // (RekapDiakui + RekapPPN) - (RekapDiskon + RekapRetur)
        [NotMapped]
        public decimal TotalRekap { get; set; }

        // Kalkulasi total pembayaran AP berdasarkan SupplierId
        [NotMapped]
        public decimal RekapDibayar { get; set; }

        // Kalkulasi sisa tagihan dari detail pembayaran AP berdasarkan SupplierId
        [NotMapped]
        public decimal SisaTagihan { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }
    }
}