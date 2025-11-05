namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.ViewModels
{
    public class IGDAssessmentAwalViewModel
    {
        public Guid? KunjunganId { get; set; }
        public bool? IsSpritualPenting { get; set; }
        public bool? IsMenngikutiKegiatanSpritual { get; set; }
        public string? DataSubjektif { get; set; }
        public string? DataObjektif { get; set; }
        public string? KebutuhanTransportasi { get; set; }
        public string? StatusKehamilan { get; set; }
        public IFormFile? TTDFile { get; set; }
        public string? TTDPath { get; set; }
    }
}
