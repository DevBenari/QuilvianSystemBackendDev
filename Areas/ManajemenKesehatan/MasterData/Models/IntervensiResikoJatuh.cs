using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstIntervensiResikoJatuh", Schema = "public")]
    public class IntervensiResikoJatuh : UserActivity
    {
        [Key]
        public Guid IntervensiResikoJatuhId { get; set; }
        public string? NamaIntervensi {  get; set; }
        public string? KategoriIntervensi { get; set; }
        public string? Keterangan {  get; set; }
    }
}
