using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("Diskon", Schema = "public")]
    public class Diskon : UserActivity
    {
        [Key]
        public Guid DiskonId { get; set; }
        public string? NamaDiskon { get; set; }
        public string? KodeVoucher { get; set; }
        public DateOnly? TglBerlaku { get; set; }
        public DateOnly? TglBerakhir { get; set; }
        public bool? IsAsuransi { get; set; }
        public Guid? AsuransiId { get; set; }
        public Guid? MetodePembayaranId { get; set; }
        public decimal? PersenDiskon { get; set; }
        public decimal? NominalDiskon { get; set; }
        public string? Keterangan { get; set; }
        public bool? IsDireksiApproved { get; set; } = false;
        public bool? IsDiskonCombined { get; set; } = false;
        public string? KategoriDiskon { get; set; }
        public int? Qty {  get; set; }
        public string? TipeDiskonDokter { get; set; }
        public int? ValueDiskonDokter { get; set; }
        public bool? IsDelete { get; set; }
    }
}
