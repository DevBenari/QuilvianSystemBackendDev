using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstGolonganDarah", Schema = "dbo")]
    public class GolonganDarah : UserActivity
    {
        [Key]
        public Guid GolonganDarahId { get; set; }
        public string KodeGolonganDarah { get; set; }
        public string NamaGolonganDarah { get; set; }
    }
}
