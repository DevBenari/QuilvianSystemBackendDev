using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models
{
    public class LabHasil : UserActivity
    {
        [Key]
        public Guid HasilLabId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? LabId { get; set; }
        public Guid? LabBookingId { get; set; }
        public List<Guid>? UserActiveId { get; set; } = new List<Guid>();
        public Guid? PenanggungJawabId { get; set; }
        public Guid? PenanggungJawabAnalisId { get; set; }
        public DateTime? TanggalPemeriksaan {  get; set; }
        public string? Keterangan {  get; set; }
    }
}
