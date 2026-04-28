using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Alkes.Models
{
    public class AlatPemakaianDetail : UserActivity
    {
        [Key]
        public Guid DetailPemakaianAlatId { get; set; }
        public Guid? PemakaianAlatId { get; set; }
        public Guid? PeralatanId { get; set; }
        public Guid? KelasId { get; set; }
        public int? QtyPemakaian {  get; set; }
        public decimal? HargaPeralatan { get; set; }
        public decimal? TotalPemakaianAlat { get; set; }
        public string? Keterangan {  get; set; }

        // Navigation Property: detail milik 1 header
        public AlatPemakaian? AlatPemakaian { get; set; }
        public Peralatan? Peralatan { get; set; }
        public Kelas? Kelas { get; set; }
    }
}
