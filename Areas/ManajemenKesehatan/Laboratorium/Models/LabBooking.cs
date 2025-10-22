using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models
{
    public class LabBooking : UserActivity
    {
        [Key]
        public Guid BookingLabId { get; set; } // Generate Otomatis
        public Guid? KunjunganId { get; set; } // Relasi dengan tabel Kunjungan
        public Guid? PasienId { get; set; } // Relasi dengan tabel Pasien
        public DateTime? TglPenyerahanSampling { get; set; } // Tanggal pengambilan atau penyerahan sampel
        public DateTime? TglBooking { get; set; } // Tanggal booking lab
        public Guid? KelasId { get; set; } // Relasi ke tabel Kelas
        public Guid? DokterId { get; set; } // Relasi ke tabel Dokter
        public string? Keterangan { get; set; } // Catatan atau keterangan tambahan
        public bool? IsCito { get; set; } // Penanda apakah pemeriksaan bersifat "Cito" (darurat)
        public string? DiagnosaAwal { get; set; }
        public Guid? DokterKonsulenId { get; set; }
        public Guid? TerapisId { get; set; }
    }
}
