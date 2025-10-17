using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models
{
    public class ResepTemplateDetail : UserActivity
    {
        [Key]
        public Guid? ResepTemplateDetailId { get; set; }
        public Guid? ResepTemplateId { get; set; }
        public Guid? AsuransiId { get; set; }
        public string? NamaAsuransi { get; set; }
        public Guid? ObatId { get; set; }
        public Guid? RacikanId { get; set; }
        public int? Qty { get; set; }
        public decimal? TakaranDosis { get; set; }
        public string? Signa { get; set; }
        public string? SignaTambahan { get; set; }
        public string? JenisObat { get; set; }
        public decimal? HargaObat { get; set; }
        public bool? StatusCoverObat { get; set; } = false;
        public bool? IsRacikan { get; set; } // "Ya" or "Tidak"
    }
}
