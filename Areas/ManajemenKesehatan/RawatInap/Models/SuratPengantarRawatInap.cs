using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    public class SuratPengantarRawatInap : UserActivity
    {
        [Key]
        public Guid SuratPengantarRawatInapId { get; set; }
        public Guid? KunjunganId { get; set; }
        public string? NomorSuratPengantar { get; set; }
        public string? Diagnosa { get; set; }
        public Guid? ICDId { get; set; }
        public string? AlasanRanap { get; set; }
        public string? RencanaTindakLanjut { get; set; }
        public string? AsalUnit { get; set; }
        public string? Status { get; set; }
    }
}
