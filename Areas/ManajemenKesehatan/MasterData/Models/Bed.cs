using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    public class Bed : UserActivity
    {
        [Key]
        public Guid BedId { get; set; }
        public Guid? KamarId { get; set; }
        public string? NomorBed { get; set; }
        public string? PosisiBed { get; set; }
        public bool? Status { get; set; }
        public string? Deskripsi { get; set; }
    }
}
