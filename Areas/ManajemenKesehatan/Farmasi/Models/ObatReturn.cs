using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models
{
    [Table("ObatReturn", Schema = "public")]
    public class ObatReturn : UserActivity
    {
        [Key]
        public Guid ObatReturnId { get; set; }
        public Guid? KasirId { get; set; }
        public Guid? ReferenceId { get; set; }
        public bool? StatusPembayaran { get; set; }
        public string? Keterangan { get; set; }
        public DateTime? TanggalReturn { get; set; }

        // Navigation
        public MainKasir? MainKasir { get; set; }

        // ICollection
        public ICollection<ObatReturnDetail> ObatReturnDetails { get; set; } = new HashSet<ObatReturnDetail>();
    }
}
