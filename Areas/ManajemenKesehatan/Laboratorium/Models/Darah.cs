using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models
{
    [Table("MstDarah", Schema = "public")]
    public class Darah : UserActivity
    {
        [Key]
        public Guid? KomponenDarahId { get; set; }
        public string? NamaKomponenDarah { get; set; }
        public string? KodeKomponenDarah { get; set; }
        public string? TipeKomponenDarah { get; set; }
        public string? Keterangan {  get; set; }
    }
}
