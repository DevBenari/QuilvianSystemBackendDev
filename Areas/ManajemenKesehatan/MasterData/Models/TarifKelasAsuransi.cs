using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstTarifKelasAsuransi", Schema = "public")]
    public class TarifKelasAsuransi : UserActivity
    {
        [Key]
        public Guid TarifKelasAsuransiId { get; set; }
        public Guid? AsuransiId { get; set; }
        public Guid? TarifKelasId { get; set; }
    }
}
