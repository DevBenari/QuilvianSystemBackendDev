using QuilvianSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystem.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstAsuransi", Schema = "dbo")]
    public class Asuransi : UserActivity
    {
        [Key]
        public Guid AsuransiId { get; set; }
        public string NamaAsuransi { get; set; }
        public string KodeAsuransi { get; set; }
        public string TipePerusahaan { get; set; }
        public string Status { get; set; }
    }
}
