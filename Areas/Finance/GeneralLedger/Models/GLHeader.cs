using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Finance.GeneralLedger.Models
{
    [Table("Fin_GLHeader", Schema = "public")]
    public class GLHeader : UserActivity
    {
        [Key]
        public Guid GLHeaderId { get; set; }

        [MaxLength(50)]
        public string? GLKode { get; set; }

        public Guid KunjunganId { get; set; }

        [MaxLength(100)]
        public string? NoRegistrasi { get; set; }

        [MaxLength(100)]
        public string? JenisKunjungan { get; set; }

        public Guid PasienId { get; set; }

        public DateTime TglTransaksi { get; set; }

        public DateTime TglPosting { get; set; }

        [MaxLength(100)]
        public string? SourceGL { get; set; }

        [MaxLength(100)]
        public string? SourceTypeGL { get; set; }

        public Guid SourceId { get; set; }

        [MaxLength(100)]
        public string? SourceNumber { get; set; }

        [MaxLength(20)]
        public string? GLStatus { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }
    }
}