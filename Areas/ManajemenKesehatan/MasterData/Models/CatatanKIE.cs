using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    public class CatatanKIE : UserActivity
    {
        [Key]
        public Guid CatKIEId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public Guid? TanggalCatat {  get; set; }
        public string? PenjelasanKIE { get; set; }
        public Guid? PerawatId { get; set; }
        public string? Keterangan {  get; set; }
    }
}
