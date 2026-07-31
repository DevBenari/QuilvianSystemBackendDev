using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    public class TindakanKunjungan : UserActivity
    {
        [Key]
        public Guid TindakanKunjunganId { get; set; }
        public Guid KunjunganId { get; set; }
        public Guid TindakanId { get; set; }
        public Guid? DepartementId { get; set; }
        public Guid? DokterPemeriksaId { get; set; }
        public Guid? KelasId {  get; set; }
        public string? TipeLayanan { get; set; }
        public DateTime? TanggalPemeriksaan { get; set; }
        public int? Quantity { get; set; }
        public decimal? Total { get; set; }
        public string? Disposition { get; set; }
        public string? Keterangan { get; set; }
        public bool? IsFoC {  get; set; }

        //Navigation 
        public Tindakan? Tindakan { get; set; }
        public Kunjungan? Kunjungan { get; set; }
        public Kelas? Kelas { get; set; }
        public Dokter? DokterPemeriksa { get; set; }

    }
}
