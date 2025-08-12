using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstDetailICD", Schema = "public")]
    public class DetailICD : UserActivity
    {
        [Key]
        public Guid DetailICDId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? SoapId { get; set; }
        public Guid? ICDId { get; set; }
        public bool? isUtama { get; set; }
    }
}
