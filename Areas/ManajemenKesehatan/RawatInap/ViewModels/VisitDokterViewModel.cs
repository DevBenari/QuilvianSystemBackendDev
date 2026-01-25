namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels
{
    public class VisitDokterViewModel
    {
        public DateTime? TanggalVisit { get; set; }
        public TimeSpan? WaktuVisit { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public Guid? KelasId { get; set; }
        public Guid? DokterId { get; set; }
        public string? Keterangan { get; set; }
    }
}
