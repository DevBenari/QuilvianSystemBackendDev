namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    public class PenerimaanDarah
    {
        public Guid PenerimaanDarahId { get; set; }
        public Guid PasienId { get; set; }
        public Guid GolonganDarahId { get; set; }
        public string? Rhesus { get; set; }
        public decimal? JumlahKantong { get; set; }
        public string? Sumber { get; set; }
        public DateTime? TglMasuk { get; set; }
        public DateTime? TglExpired { get; set; }
        public string? Keterangan { get; set; }

        // Common fields
        public Guid? CreateBy { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? DeleteBy { get; set; }
        public DateTime? DeleteDateTime { get; set; }
        public bool IsDelete { get; set; } = false;
    }

}
