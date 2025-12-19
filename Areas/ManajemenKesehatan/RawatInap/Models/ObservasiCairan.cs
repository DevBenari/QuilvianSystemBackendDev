using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    public class ObservasiCairan : UserActivity
    {
        [Key]
        public Guid ObservasiCairanId { get; set; }       // Generate Otomatis
        public Guid? KunjunganId { get; set; }             // Relasi ke tabel Kunjungan
        public Guid? PasienId { get; set; }                // Relasi ke tabel pendaftaran pasien baru
        public Guid? UserActivePerawatId { get; set; }            // Id Perawat
        public DateTime? TglObservasi { get; set; }        // Tanggal Observasi
        public string? CairanMasuk { get; set; }           // Cairan masuk
        public string? CairanKeluar { get; set; }          // Cairan keluar
        public decimal? CairanSisa { get; set; }          // Cairan sisa
        public decimal? JumlahUrin { get; set; }          // Jumlah urin
        public string? TTDPath { get; set; }               // Path file TTD (signature image)
        public string? Keterangan { get; set; }            // Catatan tambahan
    }
}
