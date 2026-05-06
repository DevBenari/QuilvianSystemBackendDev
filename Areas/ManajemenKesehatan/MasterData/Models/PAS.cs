using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstPas", Schema ="public")]
    public class PAS : UserActivity
    {
        [Key]
        public Guid? PASId { get; set; }
        public string? NamaPAS { get; set; }
        public string? Keterangan { get; set; }
    }
}
