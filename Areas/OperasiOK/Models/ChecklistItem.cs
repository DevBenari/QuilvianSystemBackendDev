using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.Operasi.Models
{
    public class ChecklistItem : UserActivity
    {
        [Key]
        public Guid ChecklistItemId { get; set; }
        public Guid? ChecklistTemplateId { get; set; }
        public decimal? UrutanChecklistItem {  get; set; }
        public string? KodeChecklistItem { get; set; }
        public string? NamaChecklistItem { get; set; }
        public string? Keterangan {  get; set; }
    }
}
