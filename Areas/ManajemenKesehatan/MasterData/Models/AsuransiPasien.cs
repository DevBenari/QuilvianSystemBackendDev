using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstAsuransiPasien", Schema = "public")]
    public class AsuransiPasien : UserActivity
    {
        [Key]
        public Guid AsuransiPasienId { get; set; }
        public Guid? PasienId { get; set; }
        public string? NoPolis { get; set; }
        public Guid? AsuransiId { get; set; }
        public bool? IsUtama { get; set; }
        public string? Umur { get; set; }
    }
}
