using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    public class VisitDokter : UserActivity
    {
        [Key]
        public Guid VisitDokterId { get; set; }
        public DateTime? TanggalVisit { get; set; }
        public TimeSpan? WaktuVisit { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public Guid? KelasId { get; set; }
        public Guid? DokterId { get; set; }
        public string? Keterangan { get; set; }
    }
}
