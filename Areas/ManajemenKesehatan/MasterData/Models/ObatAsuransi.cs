using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstObatAsuransi", Schema = "public")]
    public class ObatAsuransi
    {
        [Key]
        public Guid ObatAsuransiId { get; set; }
        public Guid ObatId { get; set; }
        public Guid AsuransiId { get; set; }
    }
}
