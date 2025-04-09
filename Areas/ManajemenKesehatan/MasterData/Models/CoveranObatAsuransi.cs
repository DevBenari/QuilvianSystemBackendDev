using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstCoveranObatAsuransi", Schema = "public")]
    public class CoveranObatAsuransi : UserActivity
    {
        [Key]
        public Guid CoveranObatAsuransiId { get; set; }
        public string KodeCoveranObat { get; set; }
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
