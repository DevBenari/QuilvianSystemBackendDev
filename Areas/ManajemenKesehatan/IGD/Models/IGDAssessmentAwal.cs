using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.Models
{
    public class IGDAssessmentAwal : UserActivity
    {
        [Key]
        public Guid AssessmentAwalIGD { get; set; }
        public Guid? KunjunganId { get; set; }
        public bool? IsSpritualPenting { get; set; }
        public bool? IsMenngikutiKegiatanSpritual {  get; set; }
        public string? DataSubjektif {  get; set; }
        public string? DataObjektif {  get; set; }
        public string? KebutuhanTransportasi { get; set; }
        public string? StatusKehamilan {  get; set; }
        public string? TTDPerawatId { get; set; }
        public string? TTDPath {  get; set; }
    }
}
