using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstPekerjaan", Schema = "dbo")]
    public class Pekerjaan : UserActivity
    {
        [Key]
        public Guid PekerjaanId { get; set; }
        public string KodePekerjaan { get; set; }
        public string NamaPekerjaan { get; set; }
    }
}
