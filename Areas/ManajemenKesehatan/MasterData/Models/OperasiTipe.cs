using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstOperasiTipe",Schema ="public")]
    public class OperasiTipe : UserActivity
    {
        [Key]
        public Guid TipeOperasiId {get; set;}
        public string? NamaTipeOperasi { get; set;}
        public string? Keterangan {  get; set;}
    }
}
