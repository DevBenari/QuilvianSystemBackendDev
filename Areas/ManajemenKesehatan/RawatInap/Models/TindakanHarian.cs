using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    public class TindakanHarian : UserActivity
    {
        [Key]
        public Guid TindakanHarianId { get; set; }   // Generate Otomatis
        public Guid? TindakanPerawatId { get; set; }  // Relasi ke tabel perawat
        public Guid? KunjunganId { get; set; }        // Relasi ke tabel kunjungan
        public Guid? PasienId { get; set; }           // Relasi ke tabel pendaftaran pasien baru
        public DateTime? TglTindakanHarian { get; set; }   // Tanggal tindakan
        public TimeOnly? WaktuTindakanHarian { get; set; } // Jam tindakan
        public string? ShiftTime { get; set; }       // Pagi / Siang / Malam
        public string? Keterangan { get; set; }      // Catatan tambahan
        public string? NamaPerawat { get; set; }     // Nama perawat
    }
}
