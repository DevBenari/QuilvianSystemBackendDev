using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.Models
{
    public class OperasiTindakan : UserActivity
    {
        [Key]
        public Guid TindakanOperasiId { get; set; }
        public Guid? TindakanId { get; set; }
        public Guid? JenisOperasiId { get; set; }
        public Guid? TipeOperasiId { get; set; }
        public string? Keterangan {  get; set; }
    }
}
