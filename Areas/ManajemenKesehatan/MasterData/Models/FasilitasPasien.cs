using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstFasilitasPasien", Schema = "public")]
    public class FasilitasPasien : UserActivity
    {
        [Key]
        public Guid FasilitasPasienId { get; set; }
        public string KodeFasilitas { get; set; }
        public string NamaFasilitasPasien { get; set; }
    }
}
