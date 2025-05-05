using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstObatAsuransi", Schema = "public")]
    public class ObatAsuransi : UserActivity
    {
        [Key]
        public Guid ObatAsuransiId { get; set; }
        public Guid ObatId { get; set; }
        public Guid AsuransiId { get; set; }
    }
}
