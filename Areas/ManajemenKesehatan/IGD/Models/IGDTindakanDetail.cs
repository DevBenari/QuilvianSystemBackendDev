using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.Models
{
    public class IGDTindakanDetail : UserActivity
    {
        [Key]
        public Guid DetailTindakanIGDId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? TindakanId {  get; set; }
        public string? Keterangan {  get; set; }
    }
}
