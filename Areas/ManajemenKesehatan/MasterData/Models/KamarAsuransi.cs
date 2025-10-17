using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("KamarAsuransi", Schema = "public")]
    public class KamarAsuransi : UserActivity
    {
        [Key]
        public Guid KamarAsuransiId { get; set; }
        public Guid? KamarId { get; set; }
        public Guid? AsuransiId { get; set; }
        public string? Keterangan { get; set; }

    }
}
