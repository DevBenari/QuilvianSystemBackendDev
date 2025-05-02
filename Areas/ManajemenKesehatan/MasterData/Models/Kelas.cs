using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstKelas", Schema = "public")]
    public class Kelas : UserActivity
    {
        [Key]
        public Guid KelasId { get; set; }
        public string? KodeKelas { get; set; }
        public string? NamaKelas { get; set; }
        public string? DeskripsiKelas { get; set; }
    }
}
