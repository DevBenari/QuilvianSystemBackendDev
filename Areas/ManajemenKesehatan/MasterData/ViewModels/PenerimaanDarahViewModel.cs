namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class PenerimaanDarahViewModel
    {
        public Guid PasienId { get; set; }
        public Guid GolonganDarahId { get; set; }
        public string? Rhesus { get; set; }
        public decimal? JumlahKantong { get; set; }
        public string? Sumber { get; set; }
        public DateTime? TglMasuk { get; set; }
        public DateTime? TglExpired { get; set; }
        public string? Keterangan { get; set; }
    }

}
