namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels
{
    public class ResikoJatuhViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public decimal? ScoreResikoJatuh { get; set; }
        public string? HasilResikoJatuh { get; set; }
        public string? ShiftPenilaian { get; set; }
        public string? Keterangan { get; set; }
    }
}
