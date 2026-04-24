using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.Models
{
    public class KulturDarah : UserActivity
    {
        [Key]
        public Guid KulturDarahId {  get; set; }
        public Guid? InfeksiId { get; set; }
        public DateTime? TglKulturDarah { get; set; }
        public string? HasilKulturDarah { get; set; }
        public string? Keterangan {  get; set; }
    }
}
