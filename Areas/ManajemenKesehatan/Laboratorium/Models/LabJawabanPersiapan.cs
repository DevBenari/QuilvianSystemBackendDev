using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models
{
    public class LabJawabanPersiapan : UserActivity
    {
        [Key]
        public Guid LabJawabanPersiapanId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public Guid? PemeriksaanLabId { get; set; }
        public Guid? LabPersiapanPemeriksaanId { get; set; }

        // Ya / Tidak
        public bool? IsJawabanPersiapan { get; set; }

        public string? Keterangan { get; set; }

        // =========================
        // Navigation Property
        // =========================

        [ForeignKey(nameof(KunjunganId))]
        public virtual Kunjungan? Kunjungan { get; set; }

        [ForeignKey(nameof(PasienId))]
        public virtual PendaftaranPasienBaru? Pasien { get; set; }

        [ForeignKey(nameof(PemeriksaanLabId))]
        public virtual LabPemeriksaan? PemeriksaanLab { get; set; }

        [ForeignKey(nameof(LabPersiapanPemeriksaanId))]
        public virtual LabPersiapanPemeriksaan? LabPersiapanPemeriksaan { get; set; }
    }
}
