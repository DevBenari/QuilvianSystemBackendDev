using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    public class TindakanKunjungan : UserActivity
    {
        [Key]
        public Guid TindakanKunjunganId { get; set; }
        public Guid KunjunganId { get; set; }
        public Guid TindakanId { get; set; }
        public Guid? RanapId { get; set; }
        public Guid? DepartementId { get; set; }
        public Guid? DokterPemeriksaId { get; set; }
        public Guid? KelasId {  get; set; }
        public DateTime? TanggalPemeriksaan { get; set; }
        public int? Quantity { get; set; }
        public decimal? Total { get; set; }
        public string? Disposition { get; set; }
        public string? Keterangan { get; set; }
    }
}
