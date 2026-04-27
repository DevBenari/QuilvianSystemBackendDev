using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstPrincipal", Schema = "public")]
    public class Principal : UserActivity
    {
        [Key]
        public Guid PrincipalId { get; set; }
        public string? NamaPrincipal { get; set; }
        public string? Keterangan { get; set; }
    }
}
