using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstTarifKelas", Schema = "public")]
    public class TarifKelas : UserActivity
    {
        [Key]
        public Guid TarifTindakanId { get; set; }
        public Guid? TindakanPoliId { get; set; }
        public Guid? PoliklinikId { get; set; }
        public string? NamaPoliklinik { get; set; }
        public Guid? KelasId { get; set; }
        public string? NamaKelas { get; set; }
        public decimal? TarifDokter { get; set; }
        public decimal? TarifRs { get; set; }
        public decimal? TarifJp { get; set; }
        public decimal? TarifBahp { get; set; }
        public decimal? TarifLain { get; set; }
        public decimal? TarifTotal { get; set; }
        public decimal? KSO { get; set; }

    }
}

