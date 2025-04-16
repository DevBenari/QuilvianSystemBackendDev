using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstSkalaPain", Schema = "public")]
    public class VitalSign : UserActivity
    {
        [Key]
        public Guid VitalSignId { get; set; }

    }
}
