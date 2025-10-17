using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    [Table("MstTopikEdukasi", Schema = "public")]
    public class TopikEdukasi : UserActivity
    {
        [Key]
        public Guid? TopikEdukasiId { get; set; }
        public string? NamaTopik {  get; set; }
        public string? Keterangan {  get; set; }
    }
}
