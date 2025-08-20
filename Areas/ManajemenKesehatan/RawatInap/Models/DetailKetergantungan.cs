using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    public class DetailKetergantungan :UserActivity
    {
        [Key]
        public Guid DetKetergantunganId { get; set; }   // Generate Otomatis
        public Guid? KunjunganId { get; set; }           // Relasi dengan tabel kunjungan
        public Guid? PengkajianPerawatId { get; set; }   // Relasi dengan tabel Pengkajian Perawat
        public Guid? KetergantunganId { get; set; }      // Relasi ke tabel ketergantungan Pengkajian
        public Guid? IndikatorPengkajianId { get; set; } // Relasi ke tabel detail ketergantungan
        public string? Keterangan { get; set; }
    }
}
