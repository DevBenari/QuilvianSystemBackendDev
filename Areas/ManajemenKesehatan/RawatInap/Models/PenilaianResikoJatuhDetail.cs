using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    public class PenilaianResikoJatuhDetail : UserActivity
    {
        [Key]
        public Guid DetailResikoJatuhId { get; set; }
        public Guid? IndikatorPengkajianId { get; set; }
        public Guid? IntervensiResikoJatuhId { get; set; }
        public string? Keterangan {  get; set; }
        public bool? IsIntervensiChecklist { get; set; }
    }
}
