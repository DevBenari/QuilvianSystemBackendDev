using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.Models
{
    public class IGDPasienDetail : UserActivity
    {
        [Key]
        public Guid DetailPasienIGDId { get; set; }
        public Guid? KunjunganId { get; set; }
        public string? JenisKasus {  get; set; }
        public string? JenisEmergency { get; set; }
        public string? KategoriPenyakit {  get; set; }
        public string? AlasanKeluar {  get; set; }
        public string? LokasiTrauma {  get; set; }
        public DateTime? TanggalTrauma { get; set; }
        public string? Keterangan {  get; set; }
    }
}
