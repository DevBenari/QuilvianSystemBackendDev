using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.Models
{
    public class RuangBedahBooking : UserActivity
    {
        [Key]
        public Guid BookingRuanganBedahId { get; set; } // Generate Otomatis
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public DateTime? TglOperasi { get; set; }
        public TimeSpan? WaktuOperasi { get; set; }
        public string? RuangTindakan { get; set; }
        public string? DiagnosaDokter1 { get; set; }
        public string? DiagnosaDokter2 { get; set; }
        public string? DiagnosaDokter3 { get; set; }
        public string? DiagnosaDokter4 { get; set; }
        public string? DiagnosaDokter5 { get; set; }
        public decimal? BeratBadan { get; set; }
        public Guid? DokterOperator1 { get; set; }
        public Guid? DokterOperator2 { get; set; }
        public Guid? DokterOperator3 { get; set; }
        public Guid? DokterOperator4 { get; set; }
        public Guid? DokterOperator5 { get; set; }
        public string? RencanaTindakanOperasi { get; set; }
        public string? JenisAnastesi { get; set; }
        public string? TypeOK { get; set; }
        public string? PenandaanLokasiOperasi { get; set; } // Belum / Sudah / Tidak Perlu
        public bool? isSuratIzinOperasi { get; set; } = false; // Default: Belum
        public bool? isBedahBersalin { get; set; } = false; // Default: false
        public string? Keterangan { get; set; }
        public DateTime? TglSelesai { get; set; }
        public bool? IsTerverifikasi {get; set; }
        public string? TipeTindakan { get; set; }
        public string? TipeOperasi { get; set; }
        public TimeOnly? JamPerpanjangan { get; set; }
        public decimal? BiayaPerpanjangan { get; set; }
        public Guid? KamarRecoveryId {  get; set; }
        public Guid? TipeAnastesiId { get; set; }
        public Guid? TipeASAId { get; set; }
        public string? KelompokPasienAnastesi { get; set; }

    }
}
