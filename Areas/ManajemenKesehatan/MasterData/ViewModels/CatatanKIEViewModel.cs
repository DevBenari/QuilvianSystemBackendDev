namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class CatatanKIEViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public Guid? TanggalCatat { get; set; }
        public string? PenjelasanKIE { get; set; }
        public Guid? PerawatId { get; set; }
        public string? Keterangan { get; set; }
    }
}
