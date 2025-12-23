using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Alkes.Models
{
    public class AlatPemakaian : UserActivity
    {
        [Key] 
        public Guid PemakaianAlatId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public DateTime? TanggalPemakaian {  get; set; }
        public string? Keterangan { get; set; }
    }
}
