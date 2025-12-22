using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Alkes.Models
{
    public class AlatPemakaianDetail : UserActivity
    {
        [Key]
        public Guid DetailPemakaianAlatId { get; set; }
        public Guid? PemakaianAlatId { get; set; }
        public Guid? PeralatanId { get; set; }
        public int? QtyPemakaian {  get; set; }
        public decimal? HargaPeralatan { get; set; }
        public decimal? TotalPemakaianAlat { get; set; }
        public decimal? Keterangan {  get; set; }
    }
}
