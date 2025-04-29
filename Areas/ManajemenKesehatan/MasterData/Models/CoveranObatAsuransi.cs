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
        public Guid? ObatId { get; set; }
        public Guid? AsuransiId { get; set; }
        public Guid? KategoriObatId { get; set; }
        public string? NamaKategoriObat { get; set; }
        public decimal? HargaRetail { get; set; }
        public string? NamaAsuransi { get; set; }
        public int? PersentaseDiskon { get; set; }
        public decimal? TarifObatAsuransi { get; set; }
        public bool? IsPKS { get; set; }
    }
}
