using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.Models
{
    public class InfeksiDetail : UserActivity
    {
        [Key]
        public Guid DetailInfeksiId { get; set; } // Generate Otomatis
        public Guid? InfeksiId { get; set; } // Relasi ke tabel Infeksi Transfusi / Infeksi Saluran / Infeksi Operasi
        public Guid? KunjunganId { get; set; } // Relasi ke tabel Kunjungan
        public Guid? PasienId { get; set; } // Relasi ke tabel Pendaftaran Pasien
        public int? HariKe { get; set; } // Setiap insert +1 jika KunjunganId dan InfeksiId sama
        public string? LokasiReaksi { get; set; } // Lokasi reaksi infeksi
        public DateTime? TglMulaiReaksi { get; set; } // Tanggal mulai reaksi
        public DateTime? TglAkhirReaksi { get; set; } // Tanggal akhir reaksi
        public string? Nyeri { get; set; } // Gejala nyeri
        public string? Merah { get; set; } // Gejala kemerahan
        public string? Bengkak { get; set; } // Gejala pembengkakan
        public string? PUS { get; set; } // Cairan nanah (PUS)
        public string? Menggigil { get; set; } // Gejala menggigil
        public bool? IsDemam { get; set; } // True jika pasien demam
        public string? Drainase { get; set; } // Catatan drainase
        public string? Perforasi { get; set; } // Catatan perforasi
        public string? Fistula { get; set; } // Catatan fistula
        public string? NyeriSupraPublik { get; set; } // Gejala nyeri suprapubik
        public string? NyeriSaatBerkemih { get; set; } // Gejala nyeri saat berkemih
        public string? PasangDCKe { get; set; } // Nomor pemasangan DC ke-
        public string? AnyangAnyangan { get; set; } // Gejala anyang-anyangan
        public string? Gatal { get; set; } // Gejala gatal
        public string? Keterangan { get; set; } // Catatan tambahan
    }
}
