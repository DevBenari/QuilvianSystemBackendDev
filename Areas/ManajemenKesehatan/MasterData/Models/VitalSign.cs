using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstVitalSign", Schema = "public")]
    public class VitalSign : UserActivity
    {
        [Key]
        public Guid VitalSignId { get; set; }
        public Guid? KunjunganId { get; set; }
        public decimal? Suhu { get; set; }
        public int? HR { get; set; }
        public int? RR { get; set; }
        public int? TekananDarahSystolic { get; set; }
        public int? TekananDarahDiastolic { get; set; }
        public decimal? SaturasiOksigen { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public decimal? BMI { get; set; }
        public decimal? LingkarKepalaBayi { get; set; }
    }
}
