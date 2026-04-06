using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstPaketLayanan", Schema = "public")]
    public class PaketLayanan : UserActivity
    {
        [Key]
        public Guid PaketLayananId { get; set; }
        public string? KodePaketLayanan { get; set; }
        public string? NamaPaketLayanan { get; set; }
        public DateTime? TglPembuatan { get; set; }
        public bool? IsMCU { get; set; }
        public string? Keterangan { get; set; }
    }
}
