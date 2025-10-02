using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    public class PengawasanHarian : UserActivity
    {
        [Key]
        public Guid PengawasanHarianId { get; set; }   // Generate Otomatis

        public Guid KunjunganId { get; set; }          // Relasi ke tabel kunjungan
        public Guid PasienId { get; set; }             // Relasi ke tabel pendaftaran pasien baru

        // Relasi ke tabel lain (hanya join untuk GET)
        public Guid? VitalSignId { get; set; }
        public Guid? PainAssessmentId { get; set; }
        public Guid? ResepId { get; set; }

        public DateTime? TglPengawasanHarian { get; set; }
        public TimeOnly? WaktuPengawasan { get; set; }

        // ✅ Aktivitas/Intervensi
        public bool? IsRelaksasi { get; set; }
        public bool? IsKompres { get; set; }
        public bool? IsDetailKompres { get; set; }   // Hangat / Dingin
        public bool? IsPijatan { get; set; }         // Sentuhan / Pijatan
        public bool? IsTens { get; set; }
        public bool? IsIstirahat { get; set; }
        public bool? IsMusik { get; set; }
        public bool? IsTeraphyAktivitas { get; set; }
        public bool? IsLatihanOtot { get; set; }

        // ✅ Intake (decimal supaya bisa pakai bilangan pecahan)
        public decimal? IntakeInfuse { get; set; }
        public decimal? IntakeOral { get; set; }
        public decimal? IntakeNGT { get; set; }
        public decimal? IntakeDarah { get; set; }
        public decimal? IntakeObat { get; set; }
        public decimal? TotalIntake { get; set; }

        // ✅ Output
        public decimal? OutputUrin { get; set; }
        public decimal? OutputFeses { get; set; }
        public decimal? OutputNGT { get; set; }
        public decimal? OutputWL { get; set; }
        public decimal? TotalOutput { get; set; }

        // ✅ Balance
        public decimal? BalanceShift { get; set; }
        public decimal? Balance24H { get; set; }

        // ✅ Parameter tambahan
        public decimal? GulaDarah { get; set; }
        public string? AsupanMakanan { get; set; }
        public string? Diet { get; set; }
        public decimal? LingkarPerut { get; set; }
        public string? MobilisasiPasien { get; set; }

        // ✅ Catatan tambahan
        public string? Keterangan { get; set; }
    }
}
