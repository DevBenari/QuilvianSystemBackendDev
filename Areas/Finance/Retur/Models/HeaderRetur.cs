using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Finance.Retur.Models
{
    [Table("Fin_HeaderRetur", Schema = "public")]
    public class HeaderRetur : UserActivity
    {
        [Key]
        public Guid HeaderReturId { get; set; } = Guid.NewGuid();

        public Guid SupplierId { get; set; }

        [NotMapped]
        public string? NamaSupplier { get; set; }

        public Guid GudangId { get; set; }

        [NotMapped]
        public string? NamaGudang { get; set; }

        [MaxLength(50)]
        public string? KodeRetur { get; set; }

        [MaxLength(50)]
        public string? StatusRetur { get; set; }

        public bool IsTerkonfirmasi { get; set; }

        public DateTime TglRetur { get; set; }

        public string? Keterangan { get; set; }
    }
}