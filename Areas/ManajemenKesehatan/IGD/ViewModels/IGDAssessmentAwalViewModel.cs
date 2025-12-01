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
        public Guid? TTDPerawatId { get; set; }
        public string? Pemeriksaan { get; set; }
        public IFormFile? GambarPenandaan { get; set; }
        public string? KondisiUmum { get; set; }
        public Guid? HasilLabId { get; set; }
        public string? Diagnosa { get; set; }
        public string? TanggalPencatatan { get; set; }
        public string? HasilAlloanamnesis { get; set; }
        public bool? IsAnamnesis { get; set; }
    }
}
