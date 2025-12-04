using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models
{
    public class ObatSubstitusi : UserActivity
    {
        [Key]
        public Guid SubstitusiObatId { get; set; }   // Generate otomatis
        public Guid KunjunganId { get; set; }
        public Guid PasienId { get; set; }
        public Guid ResepId { get; set; }
        public Guid? PengambilObatId { get; set; }   // ID apoteker yang mengambil obat
        public Guid? PengemasObatId { get; set; }    // ID apoteker yang mengemas obat
        public DateTime? WaktuAccDokter { get; set; } // Dokter approval time
        public Guid? DokterAccId { get; set; }        // Dokter yang ACC
        public string? Keterangan { get; set; }
    }
}
