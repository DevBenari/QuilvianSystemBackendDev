using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstDokterAsuransi", Schema = "public")]
    public class DokterAsuransi : UserActivity
    {
        [Key]
        public Guid DokterAsuransiId { get; set; }
        public Guid DokterId { get; set; }
        public Guid AsuransiId { get; set; }

        // Navigation properties
        //public virtual Asuransi? Asuransi { get; set; }
    }
}
