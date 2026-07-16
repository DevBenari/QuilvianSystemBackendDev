using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Finance.Pajak.Models
{
    [Table("PajakRelasis")]
    public class PajakRelasi
    {
        [Key]
        public Guid PajakRelasiId { get; set; }

        // Disimpan sebagai ID referensi saja. Tidak menggunakan foreign key constraint.
        public Guid PajakId { get; set; }

        [Required]
        [MaxLength(30)]
        public string JenisRelasi { get; set; } = string.Empty;

        // ID karyawan, dokter, perusahaan/vendor, asuransi, atau BPJS.
        // Dipilih berdasarkan nilai JenisRelasi dan digunakan saat JOIN di query.
        public Guid RelasiId { get; set; }

        [MaxLength(50)]
        public string? JenisTransaksi { get; set; }

        public DateOnly TanggalMulai { get; set; }
        public DateOnly? TanggalBerakhir { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? Keterangan { get; set; }

        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
    }
}
