using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstTarifKelas", Schema = "public")]
    public class TarifKelas : UserActivity
    {
        [Key]
        public Guid TarifKelasId { get; set; }
        public Guid? TindakanId { get; set; }
        public Guid? KelasId { get; set; }
        public Guid? PemeriksaanLabId { get; set; }
        public Guid? PeralatanId { get; set; }
        public Guid? DokterId { get; set; }
        public decimal? TarifDokter { get; set; }
        public decimal? TarifRs { get; set; }
        public decimal? TarifJp { get; set; }
        public decimal? TarifBahp { get; set; }
        public decimal? TarifLain { get; set; }
        public decimal? TarifTotal { get; set; }
        public decimal? KSO { get; set; }


    }
}