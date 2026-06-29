using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Finance.Pembayaran.Models
{
    [Table("Fin_DetailPembayaranManual", Schema = "public")]
    public class DetailPembayaranManual : UserActivity
    {
        [Key]
        public Guid DetailPembayaranManualId { get; set; } = Guid.NewGuid();

        public Guid PembayaranManualId { get; set; }

        public Guid CoaId { get; set; }

        [Required]
        [MaxLength(500)]
        public string? DeskripsiPembayaran { get; set; }

        public Guid CostCenterId { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal NominalPembayaran { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }

        public PembayaranManual? PembayaranManual { get; set; }
    }
}