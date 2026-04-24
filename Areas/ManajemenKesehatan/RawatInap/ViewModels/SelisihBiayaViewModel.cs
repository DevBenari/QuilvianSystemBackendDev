namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels
{
    public class SelisihBiayaViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public string? NamaPasien { get; set; }
        public string? AlamatPasien { get; set; }
        public string? NoRM { get; set; }
        public string? Kelas { get; set; }
        public string? NamaPenandaTangan { get; set; }
        public string? AlamatPenandaTangan { get; set; }
        public string? PekerjaanPenandaTangan { get; set; }
        public string? NoPengenalPenandaTangan { get; set; }
        public string? TipeTandaPengenal { get; set; }
        public string? NoHpPenandaTangan { get; set; }
        public string? NoTelpKantorPenandaTangan { get; set; }
        public string? HubunganPasien { get; set; }
        public DateTime? TanggalTTD { get; set; }
        public Guid? PetugasId { get; set; }
        public IFormFile? TTDPenandaTangan { get; set; }
        public string? Keterangan { get; set; }
    }
}
