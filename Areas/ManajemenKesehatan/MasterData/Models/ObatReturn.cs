using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
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
    }
}
