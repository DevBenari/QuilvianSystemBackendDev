using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstDetailResep", Schema = "public")]
    public class DetailResep : UserActivity
    {
        [Key]
        public Guid DetailResepId { get; set; }
        public Guid? ResepId { get; set; }
        public Guid? AsuransiId { get; set; }
        public string? NamaAsuransi { get; set; }
        public Guid? ObatId { get; set; }
        public int? Qty { get; set; }
        public string? Signa { get; set; }
        public string? SignaTambahan { get; set; }
        public string? InteraturObat { get; set; }
        public string? JenisObat { get; set; }
        public decimal? HargaObat { get; set; }
        public bool? StatusCoverObat { get; set; } = false;
    }
}
