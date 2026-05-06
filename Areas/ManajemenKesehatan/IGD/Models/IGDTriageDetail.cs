using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.Models
{
    public class IGDTriageDetail : UserActivity
    {
        [Key]
        public Guid? DetailTriageId { get; set; }
        public Guid? TriageId { get; set; }
        public Guid? IndikatorPengkajianId { get; set; }
        public string? Keterangan {  get; set; }
    }
}
