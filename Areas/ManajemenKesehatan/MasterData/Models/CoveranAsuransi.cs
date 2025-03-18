using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstCoveranAsuransi", Schema = "public")]
    public class CoveranAsuransi :UserActivity
    {
        [Key]
        public Guid CoveranAsuransiId { get; set; }
        [Required]
        public string KodeCoveranAsuransi { get; set; }
        public string? NamaAsuransi { get; set; }
        public string? ServiceCode { get; set; }
        public string? ServiceDesc { get; set; }
        public string? ServiceCodeClass { get; set; }
        public string? Class { get; set; }
        public bool? IsSurgery { get; set; }
        public decimal? Tarif { get; set; }
        public string? TglBerlaku { get; set; }
        public string? TglBerakhir { get; set; }
        public bool? IsPKS { get; set; }

        //relasi ke Asuransi
        [ForeignKey("AsuransiId")]
        public Guid? AsuransiId { get; set; }
    }
}
