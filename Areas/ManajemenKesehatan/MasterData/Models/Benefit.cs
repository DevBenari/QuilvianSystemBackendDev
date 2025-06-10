using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstBenefit", Schema = "public")]
    public class Benefit : UserActivity
    {
        [Key]
        public Guid BenefitId { get; set; }
        public string? NamaBenefit { get; set; }
        public string? Keterangan { get; set; }
        public decimal? BiayaBenefit { get; set; }
        public bool? IsAktif { get; set; } = true;

    }
}
