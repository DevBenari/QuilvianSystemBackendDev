using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstDokterPraktek", Schema = "public")]
    public class DokterPraktek : UserActivity
    {
        [Key]
        public Guid DokterPraktekId { get; set; }
        public string Dokter { get; set; }
        public string Layanan { get; set; }
        public string JamPraktek { get; set; }
        public string Hari { get; set; }
        public DateTime? JamMasuk { get; set; }
        public DateTime? JamKeluar { get; set; }

        // RElasi
        public Guid DokterId { get; set; }

        [ForeignKey("DokterId")]
        public Dokter Dokters { get; set; }
    }
}
