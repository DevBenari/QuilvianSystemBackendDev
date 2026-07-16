using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Finance.Pajak.Models
{
    [Table("Pajaks")]
    public class Pajak
    {
        [Key]
        public Guid PajakId { get; set; }

        [Required]
        [MaxLength(30)]
        public string KodePajak { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string NamaPajak { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string JenisPajak { get; set; } = string.Empty;

        [Column(TypeName = "numeric(8,4)")]
        public decimal TarifPersen { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
    }
}
