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
        public string? KodeResepTemplate { get; set; }
        public string? Judul { get; set; }
        public string? Diagnosa { get; set; }
        public string? Deskripsi { get; set; }
        public Guid? DokterId { get; set; }
    }
}
