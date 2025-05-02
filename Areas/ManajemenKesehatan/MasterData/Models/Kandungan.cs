using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstKandungan", Schema = "public")]
    public class Kandungan
    {
        [Key]
        public Guid KandunganId { get; set; }
        public string KodeKandungan { get; set; }
        public string NamaKandungan { get; set; }
    }
}
