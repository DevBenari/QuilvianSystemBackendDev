using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    public class EvaluasiAwal : UserActivity
    {
        [Key]
        public Guid EvaluasiAwalId { get; set; }   // Generate Otomatis
        public Guid? KunjunganId { get; set; }      // Relasi dengan tabel Kunjungan
        public Guid? PasienId { get; set; }         // Relasi dengan tabel Pendaftaran Pasien Baru
        public string? KekuatanKemampuan { get; set; }   // Fisik, Fungsional, Kognitif, Kemandirian
        public string? RiwayatKesehatan { get; set; }
        public string? KesehatanMental { get; set; }
        public string? TersedianyaDukungan { get; set; }
        public string? FinancialEvaluasiAwal { get; set; }
        public  Guid? AsuransiId { get; set; }          // Relasi dengan tabel Asuransi (nullable kalau opsional)
        public string? RiwayatObatAlternatif { get; set; }
        public string? RiwayatTrauma { get; set; }
        public string? HarapanHasil { get; set; }       // Harapan hasil asuhan, kemampuan menerima perubahan
        public string? AspekLegal { get; set; }
        public string? DischargePlanning { get; set; }
        public string? KebutuhanLain { get; set; }
        public string? Keterangan { get; set; }
        public DateTime? TglEvaluasiAwal { get; set; }
    }
}
