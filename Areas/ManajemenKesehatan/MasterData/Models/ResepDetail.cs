using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstResepDetail", Schema = "public")]
    public class ResepDetail : UserActivity
    {
        [Key]
        public Guid DetailResepId { get; set; }
        public Guid? ResepId { get; set; }
        public Guid? AsuransiId { get; set; }
        public Guid? RacikanId { get; set; }
        public string? IsRacikan { get; set; } // "Ya" or "Tidak"
        public string? NamaAsuransi { get; set; }
        public Guid? ObatId { get; set; }
        public int? Qty { get; set; }
        public string? Signa { get; set; }
        public string? SignaTambahan { get; set; }
        public string? JenisObat { get; set; }
        public decimal? HargaObat { get; set; }
        public decimal? TotalHargaObat { get; set; }
        public bool? StatusCoverObat { get; set; } = false;
    }
}
