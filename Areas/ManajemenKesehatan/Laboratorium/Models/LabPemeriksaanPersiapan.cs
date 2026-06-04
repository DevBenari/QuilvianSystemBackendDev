using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models
{
    public class LabPemeriksaanPersiapan : UserActivity
    {
        [Key]
        public Guid LabPemeriksaanPersiapanId { get; set; }
        public Guid? LabId { get; set; }
        public Guid? PemeriksaanLabId { get; set; }

        public Guid? LabPersiapanPemeriksaanId { get; set; }
        public string? Keterangan { get; set; }

        // =========================
        // Navigation Property
        // =========================

        [ForeignKey(nameof(LabId))]
        public virtual Lab? Lab { get; set; }

        [ForeignKey(nameof(PemeriksaanLabId))]
        public virtual LabPemeriksaan? LabPemeriksaan { get; set; }

        [ForeignKey(nameof(LabPersiapanPemeriksaanId))]
        public virtual LabPersiapanPemeriksaan? LabPersiapanPemeriksaan { get; set; }
    }
}
