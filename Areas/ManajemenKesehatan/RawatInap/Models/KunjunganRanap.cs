using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.X509Certificates;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    [Table("KunjunganRanap", Schema = "public")]
    public class KunjunganRanap : UserActivity
    {
        [Key]
        public Guid KunjunganRanapId { get; set; }
        public Guid? PasienId { get; set; }
        public Guid? DokterDPJPId { get; set; }
        public string? TipePembayaran { get; set; }
        public bool? StatusRanap { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? SuratPengantarId { get; set; }
        public Guid? BedId { get; set; }
        public string? KeteranganSelesaiRanap { get; set; }
        public bool? IsPrioritas { get; set; }
        public bool? IsCito { get; set; }
        public DateTime? TglAdministrasi { get; set; }
        public string? KodeKunjungan { get; set; }
        public Guid? AsuransiId { get; set; }
        public string? Keterangan { get; set; }
    }
}
