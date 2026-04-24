using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    public class TransferPasienDetail : UserActivity
    {
        [Key]
        public Guid DetailTransferPasienId { get; set; } // Generate Otomatis
        public Guid? PemeriksaanLabId { get; set; }
        public Guid? TransferPasienId { get; set; }
        public Guid? LabId { get; set; }
        public string? PenggunaanAlat { get; set; }
        public DateTime? TglPasang { get; set; } // Tgl ketika alat digunakan/dipasang
        public DateTime? TglPemeriksaanLab { get; set; }
        public decimal? JumlahPemeriksaanLab { get; set; }
        public string? Keterangan { get; set; }
    }
}
