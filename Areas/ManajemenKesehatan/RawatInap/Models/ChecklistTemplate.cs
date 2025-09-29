using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    [Table("MstChecklistTemplate", Schema = "public")]
    public class ChecklistTemplate : UserActivity
    {
        [Key]
        public Guid ChecklistTemplateId { get; set; }
        public string? NamaTemplateChecklist {  get; set; }
        public string? Keterangan {  get; set; }
    }
}
