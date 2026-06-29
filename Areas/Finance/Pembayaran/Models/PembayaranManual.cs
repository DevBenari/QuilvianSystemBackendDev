using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Finance.Pembayaran.Models
{
    [Table("Fin_PembayaranManual", Schema = "public")]
    public class PembayaranManual : UserActivity
    {
        [Key]
        public Guid PembayaranManualId { get; set; } = Guid.NewGuid();

        [MaxLength(100)]
        public string? KodePembayaranManual { get; set; }

        public DateTime? TglDokumen { get; set; }

        public DateTime? TglPembayaranManual { get; set; }

        public Guid? MataUangId { get; set; }

        public Guid? ExchangeRateId { get; set; }

        public DateTime? TglJatuhTempo { get; set; }

        public Guid? SupplierId { get; set; }

        // GET dari master supplier by SupplierId
        [NotMapped]
        public string? SupplierNama { get; set; }

        // GET dari master supplier by SupplierId
        [NotMapped]
        public string? PPN { get; set; }

        // COA Pajak
        public Guid? PajakId { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal? PersenanPajak { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal? NominalPajak { get; set; }

        [MaxLength(100)]
        public string? NomorFakturPajak { get; set; }

        public DateTime? TglFakturPajak { get; set; }

        // Tidak harus diisi
        public Guid? PoId { get; set; }

        [MaxLength(100)]
        public string? NoReferensiManual { get; set; }

        [MaxLength(100)]
        public string? StatusPembayaranManual { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }

        public List<DetailPembayaranManual>? DetailPembayaranManuals { get; set; }
    }
}