using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.Models
{
    public class RuangBedahBookingDetail : UserActivity
    {
        [Key]
        public Guid DetailBookingBedahId { get; set; }            // Generate Otomatis
        public Guid? BookingRuanganBedahId { get; set; }           // Relasi ke Booking Ruangan Bedah
        public Guid? JenisOperasiId { get; set; }                  // Relasi ke Jenis Operasi
        public List<Guid>? TindakanId { get; set; } = new List<Guid>();                   // Relasi ke Tindakan
        public List<Guid>? UserActiveId { get; set; } = new List<Guid>();
        public decimal? PersentaseTindakan { get; set; }           // Dalam Persentase
        public decimal? DiskonDokter { get; set; }                 // Dalam Persentase
        public string? Keterangan { get; set; }
    }
}
