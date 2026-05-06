using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MCU.Models
{
    public class PaketMCU : UserActivity
    {
        [Key]
        public Guid PaketMCUId { get; set; }
        public Guid? PemeriksaanLabId { get; set; }
        public Guid? ModulMCUId { get; set; }
        public Guid? DokterID { get; set; }
        public string? Keterangan { get; set; }
    }
}
