using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.OperasiOK.Models
{
    public class ChecklistResponse : UserActivity
    {
        [Key]
        public Guid ChecklistResponseId { get; set; }
        public Guid? ChecklistItemId { get; set; }
        public Guid? PraOperasiId { get; set; }

        public bool? RoleAnswers { get; set; }
        public bool? ChecklistAnswers { get; set; }

        public Guid? AnswersId { get; set; }
        public string? Keterangan { get; set; }
    }
}
