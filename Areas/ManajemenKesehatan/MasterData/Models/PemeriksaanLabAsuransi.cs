using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstPemeriksaanAsuransi", Schema = "public")]
    public class PemeriksaanLabAsuransi : UserActivity
    {
        [Key]
        public Guid PemeriksaanLabAsuransiId { get; set; }
        public Guid? PemeriksaanLabId { get; set; }
        public Guid? AsuransiId { get; set; }
        public decimal? Diskon { get; set; }
    }
}
