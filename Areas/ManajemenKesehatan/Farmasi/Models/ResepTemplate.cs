using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models
{
    [Table("MstResepTemplate", Schema = "public")]
    public class ResepTemplate : UserActivity
    {
        [Key]
        public Guid ResepTemplateId { get; set; }
        public Guid? ObatId { get; set; }
        public Obat Obat { get; set; }
        public string? KodeResepTemplate { get; set; }
        public string? Judul { get; set; }
        public Guid? DokterId { get; set; }
        public int? Qty { get; set; }
        public string? Signa { get; set; }
        public string? SignaTambahan { get; set; }
        public string? InteraturObat { get; set; }
    }
}
