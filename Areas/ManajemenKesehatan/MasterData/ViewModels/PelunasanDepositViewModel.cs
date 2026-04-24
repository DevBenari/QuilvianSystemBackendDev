namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class PelunasanDepositViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public DateTime? TanggalTTD { get; set; }
        public string? NamaPenandaTangan { get; set; }
        public string? AlamatPenandaTangan { get; set; }
        public string? TelpPenandaTangan { get; set; }
        public DateTime? TanggalJatuhTempo { get; set; }
        public IFormFile? TTDPenandaTangan { get; set; } 
        public Guid? PetugasId { get; set; }
        public string? Keterangan { get; set; }
    }
}
