using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models
{
    public class LabPersiapanPemeriksaan : UserActivity
    {
        [Key]
        public Guid LabPersiapanPemeriksaanId { get; set; }

        public string? PersiapanPemeriksaan { get; set; }

        // Screening / Instruksi / Pertanyaan
        public string? TipePersiapan { get; set; }

        // Jika true, berarti persiapan pemeriksaan ini butuh detail/jawaban tambahan
        public bool? IsDetailPersiapan { get; set; }

        public string? Keterangan { get; set; }
        // =========================
        // Navigation Property
        // =========================

        public virtual ICollection<LabPemeriksaanPersiapan> LabPemeriksaanPersiapans { get; set; }
            = new HashSet<LabPemeriksaanPersiapan>();

        public virtual ICollection<LabJawabanPersiapan> LabJawabanPersiapans { get; set; }
            = new HashSet<LabJawabanPersiapan>();
    }
}
